import React, { useContext } from "react";

import { Route, RouteProps, useRouteMatch } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { IRouteParams, RoleName } from "./types";

interface ConditionalRouteProps extends RouteProps {
  roles: RoleName[];
}

export const ConditionalRoute = (props: ConditionalRouteProps) => {
  const [context] = useContext(AppContext);
  const match = useRouteMatch<IRouteParams>("/:cluster/:path");
  const cluster = match?.params.cluster;

  // if the user has System role they can see everything (But we don't have a roles table yet)
  const systemUsers = ["jsylvest", "postit", "cydoval", "sweber"];
  if (systemUsers.includes(context.user.detail.kerberos)) {
    return <Route {...props} />;
  }

  // Not an admin, determine if they have sufficient permissions within this cluster
  const clusterAccount = context.accounts.find((a) => a.cluster === cluster);

  if (clusterAccount) {
    if (props.roles.includes("Admin")) {
      if (clusterAccount.isAdmin) {
        return <Route {...props} />;
      }
    }

    if (props.roles.includes("Sponsor")) {
      if (clusterAccount.canSponsor) {
        return <Route {...props} />;
      }
    }
  }

  return <Route {...props} component={Restricted}></Route>;
};

const Restricted = () => (
  <div>Sorry, you don't have access to see this page</div>
);
