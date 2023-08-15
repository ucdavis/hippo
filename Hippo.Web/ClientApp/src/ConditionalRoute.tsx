import React, { useContext } from "react";

import { Route, RouteProps, useRouteMatch } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { IRouteParams, RoleName } from "./types";

interface ConditionalRouteProps extends RouteProps {
  roles: RoleName[];
}

export const ConditionalRoute = (props: ConditionalRouteProps) => {
  const { roles } = props;
  const [context] = useContext(AppContext);
  const match = useRouteMatch<IRouteParams>("/:cluster/:path");
  const cluster = match?.params.cluster;

  // system admins can access anything
  if (context.user.permissions.some((p) => p.role === "System")) {
    return <Route {...props} />;
  }

  // if no non-system roles are specified, then no one else can access this route
  if (!roles.some((r) => r !== "System")) {
    return <Route {...props} component={Restricted}></Route>;
  }

  // remaining roles require cluster to be set
  if (!Boolean(cluster)) {
    return <Route {...props} component={Restricted}></Route>;
  }

  // cluster admins can access anything in their cluster
  if (
    context.user.permissions.some(
      (p) => p.cluster === cluster && p.role === "ClusterAdmin"
    )
  ) {
    return <Route {...props} />;
  }

  // if no non-cluster-admin roles are specified, then no one else can access this route
  if (!roles.some((r) => r !== "ClusterAdmin")) {
    return <Route {...props} component={Restricted}></Route>;
  }

  // check for any remaining cluster-specific roles
  if (
    context.user.permissions.some(
      (p) => p.cluster === cluster && roles.includes(p.role)
    )
  ) {
    return <Route {...props} />;
  }

  return <Route {...props} component={Restricted}></Route>;
};

const Restricted = () => (
  <div>Sorry, you don't have access to see this page</div>
);
