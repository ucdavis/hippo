/**
 * @laholstege TODO: get real descriptions for each status
 * @enum
 * @description Represents the different statuses an order can have.
 * @property Draft - The order is in draft status. This is only a client-side status, and is represented by an empty string.
 * @property Created - The order has been created by a user, but not yet submitted for processing. Sponsor can edit.
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

export const adminCanArchiveStatuses = [OrderStatus.Completed];
