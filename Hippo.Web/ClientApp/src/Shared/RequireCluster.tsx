import { useContext } from "react";
import { Navigate, Outlet, useParams } from "react-router-dom";
import AppContext from "./AppContext";

export const RequireCluster = () => {
  const [{ clusters }] = useContext(AppContext);
  const { cluster } = useParams();

  // Administrative permissions do not require an account on this cluster.
  return clusters.some((c) => c.name === cluster) ? (
    <Outlet />
  ) : (
    <Navigate to="/clusters" replace />
  );
};
