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

export type AccessType = "SshKey" | "OpenOnDemand";

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

export interface RawRequestModel {
  id: number;
  requesterEmail: string;
  requesterName: string;
  groupModel: GroupModel;
  status: "PendingApproval" | "Rejected" | "Processing" | "Completed";
  cluster: string;
  action: string;
  data: string;
}

type RequestModelCommon = Omit<RawRequestModel, "action" | "data">;

// action-specific RequestModel fields defined here...
export interface AccountRequestDataModel {
  supervisingPI: string;
  sshKey?: string;
  accessTypes: AccessType[];
}
export type AccountRequestModel = RequestModelCommon & {
  action: "CreateAccount" | "AddAccountToGroup";
  data: AccountRequestDataModel;
};

// make RequestMode a union of all possible action-specific RequestModels (currently only one)
export type RequestModel = AccountRequestModel;

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
  accessTypes: AccessType[];
}

export interface AccountCreateModel {
  groupId: number;
  sshKey: string;
  supervisingPI: string;
  accessTypes: AccessType[];
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
  clusters: ClusterModel[];
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
  id: number;
  name: string;
  description: string;
  sshName: string;
  sshKeyId: string;
  sshUrl: string;
  domain: string;
  email: string;
  accessTypes: AccessType[];
  sshKey?: string;
}

export type ModelState = Record<string, string>;
export type BadRequest = string | ModelState;
