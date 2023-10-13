import { useContext } from "react";
import { Redirect, useParams } from "react-router-dom";
import AppContext from "../Shared/AppContext";
import { IRouteParams } from "../types";

// redirect to the proper page depending on current account status
export const Home = () => {
  const [{ accounts, clusters }] = useContext(AppContext);

  if (accounts.length === 0) {
    // no accounts and no cluster selected

    // since we only have one cluster for now, let's just redirect to it
    if (clusters.length === 1) {
      return <Redirect to={`/${clusters[0].name}/create`} />;
    } else {
      // if we ever get more than one, we need to start with the cluster selection
      return <Redirect to="/clusters" />;
    }
  } else if (accounts.length === 1) {
    // one account, show page depending on status
    return <Redirect to={`/${accounts[0].cluster}/myaccount`} />;
  } else {
    return <Redirect to="/clusters" />;
  }
};

// redirect to the proper page depending on current account status within specific cluster
export const ClusterHome = () => {
  const [{ accounts, clusters }] = useContext(AppContext);
  const { cluster } = useParams<IRouteParams>();

  // first, let's make sure this is a valid cluster we support
  var validCluster = clusters.some((c) => c.name === cluster);

  if (!validCluster) {
    return <Redirect to="/clusters" />;
  }

  const accountInCluster = accounts.find((a) => a.cluster === cluster);

  if (accountInCluster === undefined) {
    // no accounts, show request form for this cluster
    return <Redirect to={`/${cluster}/create`} />;
  } else {
    // one account, show page depending on status
    return <Redirect to={`/${accountInCluster.cluster}/myaccount`} />;
  }
};
