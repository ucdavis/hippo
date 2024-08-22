/**
 * @enum
 * @description Represents the different statuses an order can have.
 * @property Draft - The order is in draft status. This is only a client-side status, and is represented by an empty string.
 * @property Created - The order has been created for a user by an admin, but not yet submitted for processing. Sponsor can edit and must approve.
 * @property Submitted - The order has been submitted.
 * @property Processing - The order is being processed. Admins can still edit the order.
 * @property Cancelled - The order has been cancelled.
 * @property Active - The order is active. Admins can still edit the order.
 * @property Rejected - The order has been rejected.
 * @property Completed - The order has been completed.
 * @property Archived - The order has been archived.
 */
export enum OrderStatus {
  Draft = "",
  Created = "Created",
  Submitted = "Submitted",
  Processing = "Processing",
  Cancelled = "Cancelled",
  Active = "Active",
  Rejected = "Rejected",
  Closed = "Closed",
  Completed = "Completed",
  Archived = "Archived",
}

/**
 * @returns negative if `a` is before `b`, positive if `a` is after `b`, and 0 if they are the same.
 */
export const compareOrderStatus = (a: OrderStatus, b: OrderStatus): number => {
  const orderStatuses = Object.values(OrderStatus);
  return orderStatuses.indexOf(a) - orderStatuses.indexOf(b);
};

/**
 * @param status - The status to get the value of.
 * @returns A numeric value representing the status. This is used for sorting statuses and displaying progress bars.
 */
export const statusValue = (status: OrderStatus) => {
  switch (status) {
    case OrderStatus.Created:
      return 1;
    case OrderStatus.Submitted:
      return 2;
    case OrderStatus.Processing:
      return 3;
    case OrderStatus.Cancelled:
      return 2.5;
    case OrderStatus.Active:
      return 4;
    case OrderStatus.Completed:
      return 5;
    case OrderStatus.Rejected:
      return 5;
    case OrderStatus.Archived:
      return 5;
    case OrderStatus.Closed:
      return 5;
    default:
      return 0;
  }
};

export interface UpdateOrderStatusModel {
  currentStatus: OrderStatus;
  newStatus: OrderStatus;
}

export const getNextStatus = ({
  status,
  isRecurring,
}: {
  status: OrderStatus;
  isRecurring?: boolean;
}): UpdateOrderStatusModel => {
  switch (status) {
    case OrderStatus.Draft:
      return {
        currentStatus: status,
        newStatus: OrderStatus.Created,
      };
    case OrderStatus.Created:
      return {
        currentStatus: status,
        newStatus: OrderStatus.Submitted,
      };
    case OrderStatus.Submitted:
      return {
        currentStatus: status,
        newStatus: OrderStatus.Processing,
      };
    case OrderStatus.Processing:
      return {
        currentStatus: status,
        newStatus: OrderStatus.Active,
      };
    case OrderStatus.Active:
      if (isRecurring) {
        return {
          currentStatus: status,
          newStatus: OrderStatus.Closed,
        };
      }
      break;
    case OrderStatus.Completed:
      return {
        currentStatus: status,
        newStatus: OrderStatus.Archived,
      };
    default:
      return {
        currentStatus: status,
        newStatus: status,
      };
  }
};

/**
 * `[Created]`
 */
export const sponsorCanApproveStatuses = [OrderStatus.Created];

/**
 * `[Created]`
 */
export const sponsorEditableStatuses = [OrderStatus.Created];

/**
 * `[Created, Submitted]`
 */
export const sponsorCanCancelStatuses = [
  OrderStatus.Created,
  OrderStatus.Submitted,
];

/**
 * `[Active]`
 */
export const sponsorCanAddPaymentStatuses = [OrderStatus.Active];

/**
 * `[Processing, Active]`
 */
export const adminEditableStatuses = [
  OrderStatus.Processing,
  OrderStatus.Active,
  OrderStatus.Completed,
];

/**
 * `[Submitted, Processing]`
 */
export const adminCanApproveStatuses = [
  OrderStatus.Submitted,
  OrderStatus.Processing,
];

/**
 * `[Submitted, Processing]`
 */
export const adminCanRejectStatuses = [
  OrderStatus.Submitted,
  OrderStatus.Processing,
];

/**
 * `[Created, Submitted, Processing, Active]`
 */
export const canUpdateChartStringsStatuses = [
  OrderStatus.Created,
  OrderStatus.Submitted,
  OrderStatus.Processing,
  OrderStatus.Active,
];

export const adminCanArchiveStatuses = [
  OrderStatus.Completed,
  OrderStatus.Closed,
];

export interface OrderStatusDescriptions {
  description: string;
  adminDescription?: string;
  sponsorDescription?: string;

  sponsorCanApprove: boolean;
  sponsorCanEdit: boolean;
  sponsorCanCancel: boolean;
  sponsorCanAddPayment: boolean;

  adminCanEdit: boolean;
  adminCanApprove: boolean;
  adminCanReject: boolean;
  adminCanArchive: boolean;

  canUpdateChartStrings: boolean;
}

export const orderStatusDescriptions: Record<
  OrderStatus,
  OrderStatusDescriptions
> = {
  [OrderStatus.Draft]: {
    description: "The order has not been created yet.",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Draft),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Draft),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Draft),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Draft,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Draft),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Draft),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Draft),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Draft),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Draft,
    ),
  },
  [OrderStatus.Created]: {
    description:
      "The order has been created, but not yet submitted for processing.",
    adminDescription:
      "This will create a new order for the sponsor to review, but they must review and submit it themselves before you can work on it.",
    sponsorDescription:
      "You must review and submit this order before an admin can begin working on it.",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Created),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Created),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Created),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Created,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Created),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Created),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Created),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Created),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Created,
    ),
  },
  [OrderStatus.Submitted]: {
    description: "The order has been submitted and is awaiting processing.",
    adminDescription: "",
    sponsorDescription: "",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(
      OrderStatus.Submitted,
    ),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Submitted),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Submitted),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Submitted,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Submitted),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Submitted),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Submitted),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Submitted),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Submitted,
    ),
  },
  [OrderStatus.Processing]: {
    description: "The order is being worked on.",
    adminDescription: "You will still be able to edit the order.",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(
      OrderStatus.Processing,
    ),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Processing),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Processing),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Processing,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Processing),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Processing),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Processing),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Processing),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Processing,
    ),
  },
  [OrderStatus.Cancelled]: {
    description: "Cancelled",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(
      OrderStatus.Cancelled,
    ),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Cancelled),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Cancelled),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Cancelled,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Cancelled),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Cancelled),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Cancelled),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Cancelled),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Cancelled,
    ),
  },
  [OrderStatus.Active]: {
    description: "Active",
    adminDescription: "You will still be able to edit the order.",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Active),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Active),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Active),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Active,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Active),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Active),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Active),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Active),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Active,
    ),
  },
  [OrderStatus.Rejected]: {
    description: "Rejected",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Rejected),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Rejected),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Rejected),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Rejected,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Rejected),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Rejected),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Rejected),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Rejected),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Rejected,
    ),
  },
  [OrderStatus.Completed]: {
    description: "Completed",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(
      OrderStatus.Completed,
    ),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Completed),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Completed),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Completed,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Completed),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Completed),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Completed),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Completed),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Completed,
    ),
  },
  [OrderStatus.Archived]: {
    description: "Archived",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Archived),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Archived),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Archived),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Archived,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Archived),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Archived),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Archived),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Archived),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Archived,
    ),
  },
  [OrderStatus.Closed]: {
    description: "Closed",
    sponsorCanApprove: sponsorCanApproveStatuses.includes(OrderStatus.Closed),
    sponsorCanEdit: sponsorEditableStatuses.includes(OrderStatus.Closed),
    sponsorCanCancel: sponsorCanCancelStatuses.includes(OrderStatus.Closed),
    sponsorCanAddPayment: sponsorCanAddPaymentStatuses.includes(
      OrderStatus.Closed,
    ),
    adminCanEdit: adminEditableStatuses.includes(OrderStatus.Closed),
    adminCanApprove: adminCanApproveStatuses.includes(OrderStatus.Closed),
    adminCanReject: adminCanRejectStatuses.includes(OrderStatus.Closed),
    adminCanArchive: adminCanArchiveStatuses.includes(OrderStatus.Closed),
    canUpdateChartStrings: canUpdateChartStringsStatuses.includes(
      OrderStatus.Closed,
    ),
  },
};

export const getStatusActions = (status: OrderStatus, isAdmin: boolean) => {
  const {
    sponsorCanApprove,
    sponsorCanEdit,
    sponsorCanCancel,
    sponsorCanAddPayment,
    adminCanEdit,
    adminCanApprove,
    adminCanReject,
    adminCanArchive,
    canUpdateChartStrings,
  } = orderStatusDescriptions[status];

  return [
    sponsorCanApprove && "Approve",
    sponsorCanEdit && "Edit",
    sponsorCanCancel && "Cancel",
    sponsorCanAddPayment && "Add Payment",
    adminCanEdit && isAdmin && "Edit",
    adminCanApprove && isAdmin && "Approve",
    adminCanReject && isAdmin && "Reject",
    adminCanArchive && isAdmin && "Archive",
    canUpdateChartStrings && "Update Chart Strings",
  ]
    .filter((action) => !!action)
    .join(", ");
};
