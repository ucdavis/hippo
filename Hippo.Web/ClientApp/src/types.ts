import { ClusterNames } from "./constants";

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
  data: PuppetGroupRecord;
}

export interface RequestModelCommon {
  id: number;
  requesterEmail: string;
  requesterName: string;
  groupModel: GroupModel;
  status: "PendingApproval" | "Rejected" | "Processing" | "Completed";
  cluster: string;
}

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
  data: PuppetUserRecord;
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
  allowOrders: boolean;
}

export type ModelState = Record<string, string>;
export type BadRequest = string | ModelState;

export type ClusterName = (typeof ClusterNames)[number];

export interface SoftwareRequestModel {
  clusterName: ClusterName;
  email: string;
  accountName: string;
  accountKerberos: string;
  softwareTitle: string;
  softwareLicense: string;
  softwareHomePage: string;
  benefitDescription: string;
  additionalInformation: string;
}

// types used by puppet records...
export type DataQuota = string;
export type UInt32 = number;
export type SlurmQOSFlag = string;
export type KerberosID = string;
export type Email = string;
export type LinuxUID = UInt32;
export type LinuxGID = UInt32;
export type Shell = string;
export type PuppetAbsent = "absent";
export type PuppetEnsure = "present" | PuppetAbsent;
export type PuppetMembership = "inclusive" | "minimum";

export interface PuppetAutofs {
  nas: string;
  path: string;
  options?: string;
}

export interface PuppetZFS {
  quota: DataQuota;
}

export interface PuppetUserStorage {
  zfs: PuppetZFS | boolean;
  autofs?: PuppetAutofs;
}

export interface SlurmQOSTRES {
  cpus?: UInt32;
  gpus?: UInt32;
  mem?: DataQuota;
}

export interface SlurmQOS {
  group?: SlurmQOSTRES;
  user?: SlurmQOSTRES;
  job?: SlurmQOSTRES;
  priority?: number;
  flags?: Set<SlurmQOSFlag>;
}

export interface SlurmPartition {
  qos: SlurmQOS | string;
}

export interface SlurmRecord {
  account?: KerberosID | Set<KerberosID>;
  partitions?: Record<string, SlurmPartition>;
  max_jobs?: UInt32;
}

export interface PuppetUserRecord {
  fullname: string;
  email: Email;
  uid: LinuxUID;
  gid: LinuxGID;
  groups?: Set<KerberosID>;
  group_sudo?: KerberosID[];
  shell?: Shell;
  tag?: Set<string>;
  home?: string;
  expiry?: Date | PuppetAbsent;
  ensure?: PuppetEnsure;
  membership?: PuppetMembership;
  storage?: PuppetUserStorage;
  slurm?: SlurmRecord;
}

export interface PuppetGroupStorage {
  name: string;
  owner: KerberosID;
  group?: KerberosID;
  autofs?: PuppetAutofs;
  zfs?: PuppetZFS | boolean;
  globus?: boolean;
}

export interface PuppetGroupRecord {
  gid: LinuxGID;
  sponsors?: KerberosID[];
  ensure?: PuppetEnsure;
  tag?: Set<string>;
  storage?: PuppetGroupStorage[];
  slurm?: SlurmRecord;
}

export interface FinancialDetailModel {
  financialSystemApiKey: string;
  financialSystemApiSource: string;
  chartString: string;
  autoApprove: boolean;
  maskedApiKey: string;
}
export interface ChartStringValidationModel {
  isValid: boolean;
  description: string;
  accountManager: string;
  accountManagerEmail: string;
  message: string;
  warning: string;
}

export interface ProductBase {
  category: string;
  description: string;
  units: string;
  unitPrice: string;
  installments: number;
  installmentType: string;
  lifeCycle: number;
}

export interface ProductModel extends Partial<ProductBase> {
  id: number;
  name: string;
}

export interface OrderMetadataModel {
  id: number;
  name: string;
  value: string;
}

export interface OrderBillingModel {
  id: number;
  chartString: string;
  percentage: string;
  chartStringValidation: ChartStringValidationModel;
}

export interface PaymentModel {
  id: number;
  amount: number;
  entryAmount: string;
  status: string;
  createdOn: string;
  createdBy?: User;
  //Possibly have the chart string(s) and percent here
}
export interface HistoryModel {
  id: number;
  actedBy: User;
  action: string;
  status: string;
  details: string;
  actedDate: string;
}

export interface OrderModel extends Partial<ProductBase> {
  id: number;
  PILookup: string;
  name: string;
  productName: string;
  notes: string;
  quantity: number;
  adjustment: number;
  adjustmentReason: string;
  status: string;
  createdOn: string;
  externalReference: string;
  adminNotes: string;
  subTotal: string;
  total: string;
  balanceRemaining: string;
  piUser?: User;
  installmentDate?: string;
  expirationDate?: string;
  metaData: OrderMetadataModel[];
  billings: OrderBillingModel[];
  payments: PaymentModel[];
  history: HistoryModel[];
  percentTotal: number;
}

export interface OrderListModel {
  id: number;
  name: string;
  description: string;
  units: string;
  quantity: number;
  createdOn: string;
  status: string;
  total: string;
  balanceRemaining: string;
  sponsorName: string;
}

export interface OrderTotalCalculationModel extends Partial<OrderModel> {
  unitPrice: string;
  quantity: number;
  adjustment: number;
  subTotal: string;
  total: string;
}

export interface UpdateOrderStatusModel {
  currentStatus: string;
  newStatus: string;
}
