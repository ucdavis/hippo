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

export interface GroupAdminModel {
  permissionId: number;
  group: string;
  user: User;
}

export interface GroupUserModel {
  kerberos: string;
  name: string;
  email: string;
}

export interface GroupModel {
  id: number;
  name: string;
  displayName: string;
  admins: GroupUserModel[];
}

export interface RequestModel {
  id: number;
  requesterEmail: string;
  requesterName: string;
  action: "CreateAccount" | "AddAccountToGroup";
  groupModel: GroupModel;
}

export interface AccountModel {
  id: number;
  name: string;
  status: string;
  createdOn: string;
  cluster: string;
  owner?: User;
  groups: GroupModel[];
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
}

export interface RequestPostModel {
  groupId: number;
  sshKey: string;
}

export interface AddGroupAdminModel {
  lookup: string;
  group: string;
}

export interface AppContextShape {
  antiForgeryToken: string;
  user: {
    detail: User;
    permissions: Permission[];
  };
  accounts: AccountModel[];
  clusters: Cluster[];
}

export interface Permission {
  role: RoleName;
  cluster?: string;
  group?: string;
}

export interface PromiseStatus {
  pending: boolean;
  success: boolean;
}

export interface IRouteParams {
  cluster: string;
}

export interface ClusterModel {
  cluster: Cluster;
  sshKey?: string;
}
