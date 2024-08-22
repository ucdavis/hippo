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

export interface OrderStatusDescriptions {
  description: string;
  forAdmin?: string;
  forSponsor?: string;
}

export const orderStatusDescriptions: Record<
  OrderStatus,
  OrderStatusDescriptions
> = {
  [OrderStatus.Draft]: {
    description: "The order has not been created yet.",
    forSponsor: "If you leave this page, your order will not be saved!",
  },
  [OrderStatus.Created]: {
    description:
      "The order has been created, but not yet submitted for processing.",
    forAdmin:
      "This will create a new order for the sponsor to review, but they must review and submit it themselves before you can work on it.",
    forSponsor:
      "You must review and submit this order before an admin can begin working on it.",
  },
  [OrderStatus.Submitted]: {
    description:
      "The order has been submitted and is awaiting an admin to mark it as in process.",
  },
  [OrderStatus.Processing]: { description: "Processing" },
  [OrderStatus.Cancelled]: { description: "Cancelled" },
  [OrderStatus.Active]: { description: "Active" },
  [OrderStatus.Rejected]: { description: "Rejected" },
  [OrderStatus.Completed]: { description: "Completed" },
  [OrderStatus.Archived]: { description: "Archived" },
  [OrderStatus.Closed]: { description: "Closed" },
};

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
