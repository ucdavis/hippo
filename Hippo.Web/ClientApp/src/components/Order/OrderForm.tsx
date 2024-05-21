import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { OrderModel } from "../../types";
import FormField from "../../Shared/Form/FormField";
import { Form, Input } from "reactstrap";

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
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<OrderModel>();

  const onSubmit = (data: OrderModel, e: React.SyntheticEvent) => {
    e.preventDefault();
    console.log(data);
    console.log(errors);
  };

  // const [order, setOrder] = useState<OrderModel>(orderProp);
  // const [isReadOnly, setIsReadOnly] = useState<boolean>(readonly);

  // if (!isReadOnly) {
  //   setIsReadOnly(readonly);
  // }

  // const handleChange = (
  //   e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  // ) => {
  //   const { name, value } = e.target;
  //   setOrder((prevOrder) => ({ ...prevOrder, [name]: value }));
  //   handleChanges(e);
  // };

  // if (!order) {
  //   return <div>Loading...</div>;
  // }

  return (
    <Form onSubmit={handleSubmit(onSubmit)}>
      <FormField
        register={register}
        errors={errors}
        name="status"
        label="Status"
        required={true}
        // readOnly={readonly}
      />
      {/* <div className="form-group">
        <label htmlFor="fieldStatus">Status</label>
        <input
          className="form-control"
          id="fieldStatus"
          value={order.status}
          //readOnly
          name="status"
          onChange={handleChange}
        />
      </div> */}
      <hr />
      <FormField
        register={register}
        errors={errors}
        name="name"
        label="Name"
        required={true}
        // readOnly={readonly}
      />
      {/* <div className="form-group">
        <label htmlFor="fieldName">Name</label>
        <input
          className="form-control"
          id="fieldName"
          required
          value={order.name}
          name="name"
          onChange={handleChange}
        />
      </div> */}

      <FormField
        register={register}
        errors={errors}
        name="description"
        label="Description"
        required={true}
        // readOnly={readonly}
      />
      {/* <div className="form-group">
        <label htmlFor="fieldDescription">Description</label>
        <textarea
          className="form-control"
          id="fieldDescription"
          value={order.description || ""}
          name="description"
          onChange={handleChange}
        />
      </div> */}
      {/* <div className="form-group">
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
      </div> */}
      <Input type="submit" value="Save" />
    </Form>
  );
};

export default OrderForm;
