import { useContext } from "react";
import AppContext from "./AppContext";
import { useParams } from "react-router-dom";

export const usePermissions = () => {
  const [{ user, accounts }] = useContext(AppContext);
  const { cluster: clusterName } = useParams();

  const isSystemAdmin = user.permissions.some((p) => p.role === "System");
  const isClusterAdmin = user.permissions.some(
    (p) => p.role === "ClusterAdmin" && p.cluster === clusterName,
  );

  const canViewGroup = (groupName: string) => {
    if (isSystemAdmin || isClusterAdmin) return true;
    if (!clusterName) return false;
    const account = accounts.find((a) => a.cluster === clusterName);
    if (!account) return false;
    if (account.adminOfGroups.some((g) => g.name === groupName)) return true;
    if (account.memberOfGroups.some((g) => g.name === groupName)) return true;
    return false;
  };

  return { canViewGroup };
};
