import React, { ReactElement, useContext } from "react";

import AppContext from "./AppContext";
import { RoleName } from "../types";
import { isBoolean, isFunction } from "../util/TypeChecks";
import { useMatch } from "react-router-dom";

interface Props {
  children: any;
  roles: RoleName[];
  condition?: boolean | (() => boolean);
  alternative?: JSX.Element;
}

// Determines if the user has access to the route based on roles and cluster
export const ShowFor = (props: Props) => {
  const { children, roles, alternative } = props;
  const [context] = useContext(AppContext);
  const match = useMatch("/:cluster/:path");
  const cluster = match?.params.cluster;

  const conditionSatisfied = isBoolean(props.condition)
    ? props.condition
    : isFunction(props.condition)
    ? props.condition()
    : true;

  if (!conditionSatisfied) {
    return alternative ?? null;
  }

  // system admins can access anything
  if (context.user.permissions.some((p) => p.role === "System")) {
    return <>{children}</>;
  }

  // if no non-system roles are specified, then no one else can access this route
  if (!roles.some((r) => r !== "System")) {
    return alternative ?? null;
  }

  // remaining roles require cluster to be set
  if (!Boolean(cluster)) {
    return alternative ?? null;
  }

  // cluster admins can access anything in their cluster
  if (
    context.user.permissions.some(
      (p) => p.cluster === cluster && p.role === "ClusterAdmin"
    )
  ) {
    return <>{children}</>;
  }

  // if no non-cluster-admin roles are specified, then no one else can access this route
  if (!roles.some((r) => r !== "ClusterAdmin")) {
    return alternative ?? null;
  }

  // group admin role satisfied by presence of at least one group in account.adminOfGroups
  if (roles.find((r) => r === "GroupAdmin")) {
    const account = context.accounts.find((a) => a.cluster === cluster);
    if (account?.adminOfGroups.length) {
      return <>{children}</>;
    }
  }

  // check if user has any other cluster-specific role
  if (
    context.user.permissions.some(
      (p) => p.cluster === cluster && roles.includes(p.role)
    )
  ) {
    return <>{children}</>;
  }

  return alternative ?? null;
};

// Can be used as either a hook or a component. Exporting under
// seperate name as a reminder that rules of hooks still apply.
export const useFor = ShowFor;
