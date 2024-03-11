import { useContext, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import AppContext from "../Shared/AppContext";

// redirect to the proper page depending on current account status
export const Home = () => {
  const [{ accounts, clusters }] = useContext(AppContext);
  const navigate = useNavigate();

  useEffect(() => {
    if (accounts.length === 0) {
      // no accounts and no cluster selected

      // since we only have one cluster for now, let's just redirect to it
      if (clusters.length === 1) {
        navigate(`/${clusters[0].name}/create`);
      } else {
        // if we ever get more than one, we need to start with the cluster selection
        navigate("/clusters");
      }
    } else if (accounts.length === 1) {
      // one account, show page depending on status
      navigate(`/${accounts[0].cluster}/myaccount`);
    } else {
      navigate("/clusters");
    }
  }, [accounts, clusters, navigate]);

  return null;
};

// redirect to the proper page depending on current account status within specific cluster
export const ClusterHome = () => {
  const [{ accounts, clusters }] = useContext(AppContext);
  const { cluster } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    // first, let's make sure this is a valid cluster we support
    var validCluster = clusters.some((c) => c.name === cluster);

    if (!validCluster) {
      navigate("/clusters");
    }

    const accountInCluster = accounts.find((a) => a.cluster === cluster);

    if (accountInCluster === undefined) {
      // no accounts, show request form for this cluster
      navigate(`/${cluster}/create`);
    } else {
      // one account, show page depending on status
      navigate(`/${accountInCluster.cluster}/myaccount`);
    }
  }, [accounts, clusters, navigate, cluster]);

  return null;
};
