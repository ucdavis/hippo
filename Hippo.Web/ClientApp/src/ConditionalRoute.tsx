import React, { useContext } from "react";

import { Route, RouteProps } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { RoleName } from "./types";

interface ConditionalRouteProps extends RouteProps {
  roles: RoleName[];
}

export const ConditionalRoute = (props: ConditionalRouteProps) => {
  const [context] = useContext(AppContext);

  // if the user has System role they can see everything (But we don't have a roles table yet)
  const systemUsers = ["jsylvest", "postit", "cydoval", "sweber"];
  if (
    context.user.detail.isAdmin ||
    systemUsers.includes(context.user.detail.kerberos)
  ) {
    return <Route {...props} />;
  }

  if (props.roles.includes("Sponsor")) {
    if (context.accounts[0].canSponsor) {
      return <Route {...props} />;
    }
  }

  return <Route {...props} component={Restricted}></Route>;
};

const Restricted = () => (
  <div>Sorry, you don't have access to see this page</div>
);
