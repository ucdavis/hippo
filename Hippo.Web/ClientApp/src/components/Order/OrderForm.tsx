import React, { useState } from "react";
import { OrderModel } from "../../types";

interface OrderFormProps {
  orderProp: OrderModel;
  readonly: boolean;
  handleChanges: (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => void;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  readonly,
  handleChanges,
}) => {
  const [order, setOrder] = useState<OrderModel>(null);
  const [isReadOnly, setIsReadOnly] = useState<boolean>(readonly);

  if (!order) {
    setOrder(orderProp);
  }
  if (!isReadOnly) {
    setIsReadOnly(readonly);
  }

  if (!order) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <div className="form-group">
        <label htmlFor="fieldStatus">Status</label>
        <input
          className="form-control"
          id="fieldStatus"
          value={order.status}
          //readOnly
          name="status"
          onChange={handleChanges}
        />
      </div>
      <hr />
      <div className="form-group">
        <label htmlFor="fieldName">Name</label>
        <input
          className="form-control"
          id="fieldName"
          required
          value={order.name}
          name="name"
          onChange={handleChanges}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldDescription">Description</label>
        <textarea
          className="form-control"
          id="fieldDescription"
          value={order.description || ""}
          name="description"
          onChange={handleChanges}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldCategory">Category</label>
        <input
          className="form-control"
          id="fieldCategory"
          value={order.category}
          readOnly={isReadOnly}
          name="category"
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldExternalReference">External Reference</label>
        <input
          className="form-control"
          id="fieldExternalReference"
          value={order.externalReference}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldNotes">Notes</label>
        <textarea
          className="form-control"
          id="fieldNotes"
          value={order.notes || ""}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldUnits">Units</label>
        <input
          className="form-control"
          id="fieldUnits"
          value={order.units}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldUnitPrice">Unit Price</label>
        <div className="input-group">
          <div className="input-group-prepend">
            <span className="input-group-text" style={{ height: "38px" }}>
              <i className="fas fa-dollar-sign" />
            </span>
          </div>
          <input
            className="form-control"
            id="fieldUnitPrice"
            value={order.unitPrice}
            readOnly={isReadOnly}
          />
        </div>
      </div>
      <div className="form-group">
        <label htmlFor="fieldQuantity">Quantity</label>
        <input
          className="form-control"
          id="fieldQuantity"
          value={order.quantity}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldInstallments">Installments</label>
        <input
          className="form-control"
          id="fieldInstallments"
          value={order.installments}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldAdjustment">Adjustment</label>
        <div className="input-group">
          <div className="input-group-prepend">
            <span className="input-group-text" style={{ height: "38px" }}>
              <i className="fas fa-dollar-sign" />
            </span>
          </div>
          <input
            className="form-control"
            id="fieldAdjustment"
            value={order.adjustment}
            readOnly={isReadOnly}
          />
        </div>
      </div>
      <div className="form-group">
        <label htmlFor="fieldAdjustmentReason">Adjustment Reason</label>
        <textarea
          className="form-control"
          id="fieldAdjustmentReason"
          value={order.adjustmentReason || ""}
          readOnly={isReadOnly}
        />
      </div>
      <div className="form-group">
        <label htmlFor="fieldAdminNotes">Admin Notes</label>
        <textarea
          className="form-control"
          id="fieldAdminNotes"
          value={order.adminNotes || ""}
          readOnly={isReadOnly}
        />
      </div>
      <hr />
      <div className="form-group">
        <label htmlFor="fieldSubTotal">SubTotal</label>
        <div className="input-group">
          <div className="input-group-prepend">
            <span className="input-group-text" style={{ height: "38px" }}>
              <i className="fas fa-dollar-sign" />
            </span>
          </div>
          <div className="form-group">
            <input
              className="form-control"
              id="fieldSubTotal"
              value={order.subTotal}
              readOnly
            />
          </div>
        </div>
      </div>
      <div className="form-group">
        <label htmlFor="fieldTotal">Total</label>
        <div className="input-group">
          <div className="input-group-prepend">
            <span className="input-group-text" style={{ height: "38px" }}>
              <i className="fas fa-dollar-sign" />
            </span>
          </div>
          <input
            className="form-control"
            id="fieldTotal"
            value={order.total}
            readOnly
          />
        </div>
      </div>
    </div>
  );
};

export default OrderForm;
