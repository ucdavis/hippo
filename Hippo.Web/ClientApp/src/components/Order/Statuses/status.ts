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
    case OrderStatus.Closed:
      if (isRecurring) {
        return {
          currentStatus: status,
          newStatus: OrderStatus.Archived,
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
}

export const orderStatusDescriptions: Record<
  OrderStatus,
  OrderStatusDescriptions
> = {
  [OrderStatus.Draft]: {
    description: "The order has not been created yet.",
  },
  [OrderStatus.Created]: {
    description: "The order has been created, but not yet submitted.",
  },
  [OrderStatus.Submitted]: {
    description: "The order is awaiting approval by an admin.",
  },
  [OrderStatus.Processing]: {
    description: "The order has been approved and is being worked on.",
  },
  [OrderStatus.Cancelled]: {
    description: "The order has been cancelled by the sponsor.",
  },
  [OrderStatus.Active]: {
    description: "The order is active and billing has started.",
  },
  [OrderStatus.Rejected]: {
    description: "The order has been rejected by an admin.",
  },
  [OrderStatus.Completed]: {
    description: "The order has been completed and billing has stopped.",
  },
  [OrderStatus.Archived]: {
    description: "The order is complete and has been archived.",
  },
  [OrderStatus.Closed]: {
    description: "The recurring order has been closed and billing has stopped.",
  },
};

export const getStatusActions = ({
  status,
  isAdmin,
}: {
  status: OrderStatus;
  isAdmin: boolean;
}): { sponsorActions: string; adminActions: string } => {
  const sponsorCanApprove = sponsorCanApproveStatuses.includes(status);
  const sponsorCanEdit = sponsorEditableStatuses.includes(status);
  const sponsorCanCancel = sponsorCanCancelStatuses.includes(status);
  const sponsorCanAddPayment = sponsorCanAddPaymentStatuses.includes(status);
  const canUpdateChartStrings = canUpdateChartStringsStatuses.includes(status);

  const adminCanEdit = adminEditableStatuses.includes(status);
  const adminCanApprove = adminCanApproveStatuses.includes(status);
  const adminCanReject = adminCanRejectStatuses.includes(status);
  const adminCanArchive = adminCanArchiveStatuses.includes(status);

  return {
    sponsorActions: [
      sponsorCanApprove && "Approve",
      sponsorCanEdit && "Edit",
      sponsorCanCancel && "Cancel",
      sponsorCanAddPayment && "Add Payment",
      canUpdateChartStrings && "Update Billing Info",
    ]
      .filter((action) => !!action)
      .join(", "),
    adminActions: [
      adminCanEdit && isAdmin && "Edit",
      adminCanApprove && isAdmin && "Approve",
      adminCanReject && isAdmin && "Reject",
      adminCanArchive && isAdmin && "Archive",
      canUpdateChartStrings && isAdmin && "Update Billing Info",
    ]
      .filter((action) => !!action)
      .join(", "),
  };
};
