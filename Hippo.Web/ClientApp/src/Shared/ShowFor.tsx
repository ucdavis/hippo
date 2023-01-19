import React, { useContext } from "react";

import AppContext from "./AppContext";
import { IRouteParams, RoleName } from "../types";
import { isBoolean, isFunction } from "../util/TypeChecks";
import { useRouteMatch } from "react-router-dom";

interface Props {
  children: any;
  roles: RoleName[];
  cluster?: string;
  condition?: boolean | (() => boolean);
}

// Determines if the user has access to the route based on roles and cluster
export const ShowFor = (props: Props) => {
  const { children, roles } = props;
  const [context] = useContext(AppContext);
  const match = useRouteMatch<IRouteParams>("/:cluster/:path");
  const cluster = match?.params.cluster;

  const conditionSatisfied = isBoolean(props.condition)
    ? props.condition
    : isFunction(props.condition)
    ? props.condition()
    : true;

  if (conditionSatisfied && context.user.detail.isAdmin) {
    return <>{children}</>;
  }

  // not admin, need to check roles within the cluster
  const clusterAccount = context.accounts.find((a) => a.cluster === cluster);

  if (clusterAccount) {
    if (conditionSatisfied && roles.includes("Sponsor")) {
      if (clusterAccount.canSponsor) {
        return <>{children}</>;
      }
    }

    if (conditionSatisfied && roles.includes("Admin")) {
      if (clusterAccount.isAdmin) {
        return <>{children}</>;
      }
    }
  }

  return null;
};

// Can be used as either a hook or a component. Exporting under
// seperate name as a reminder that rules of hooks still apply.
export const useFor = ShowFor;
