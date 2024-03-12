export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
}

export type RoleName = "System" | "ClusterAdmin" | "GroupAdmin" | "GroupMember";

export interface GroupAccountModel {
  kerberos: string;
  name: string;
  email: string;
}

export interface GroupModel {
  id: number;
  name: string;
  displayName: string;
  admins: GroupAccountModel[];
}

export interface RequestModel {
  id: number;
  requesterEmail: string;
  requesterName: string;
  action: "CreateAccount" | "AddAccountToGroup";
  groupModel: GroupModel;
  status: "PendingApproval" | "Rejected" | "Processing" | "Completed";
  cluster: string;
  supervisingPI: string;
}

export interface AccountModel {
  id: number;
  name: string;
  email: string;
  kerberos: string;
  createdOn: string;
  cluster: string;
  owner?: User;
  memberOfGroups: GroupModel[];
  adminOfGroups: GroupModel[];
  updatedOn: string;
}

export interface Cluster {
  id: number;
  name: string;
  description: string;
  sshName: string;
  sshKeyId: string;
  sshUrl: string;
  domain: string;
  email: string;
  enableUserSshKey: boolean;
  enableOpenOnDemand: boolean;
}

export interface AccountCreateModel {
  groupId: number;
  sshKey: string;
  supervisingPI: string;
}

export interface AddToGroupModel {
  groupId: number;
  supervisingPI: string;
}

export interface AppContextShape {
  antiForgeryToken: string;
  user: {
    detail: User;
    permissions: Permission[];
  };
  accounts: AccountModel[];
  clusters: Cluster[];
  openRequests: RequestModel[];
  lastPuppetSync?: string;
}

export interface Permission {
  role: RoleName;
  cluster?: string;
}

export interface PromiseStatus {
  pending: boolean;
  success: boolean;
}

export interface ClusterModel {
  cluster: Cluster;
  sshKey?: string;
}
