import React, { useContext } from "react";

import AppContext from "./AppContext";
import { RoleName } from "../types";
import { isBoolean, isFunction } from "../util/TypeChecks";

interface Props {
  children: any;
  roles: RoleName[];
  cluster: string;
  condition?: boolean | (() => boolean);
}

// Determines if the user has access to the route based on roles and cluster
export const ShowFor = (props: Props) => {
  const { children, roles } = props;
  const [context] = useContext(AppContext);
  const systemUsers = ["jsylvest", "postit", "cydoval", "sweber"];
  const conditionSatisfied = isBoolean(props.condition)
    ? props.condition
    : isFunction(props.condition)
    ? props.condition()
    : true;

  if (
    conditionSatisfied &&
    systemUsers.includes(context.user.detail.kerberos)
  ) {
    return <>{children}</>;
  }

  // TODO: handle multiple accounts
  if (conditionSatisfied && roles.includes("Sponsor")) {
    if (context.accounts[0].canSponsor) {
      return <>{children}</>;
    }
  }

  if (conditionSatisfied && roles.includes("Admin")) {
    if (context.user.detail.isAdmin) {
      return <>{children}</>;
    }
  }

  return null;
};

// Can be used as either a hook or a component. Exporting under
// seperate name as a reminder that rules of hooks still apply.
export const useFor = ShowFor;
