import React from "react";
import { useForm } from "react-hook-form";
import { OrderModel } from "../../types";
import FormField from "../../Shared/Form/FormField";
import { Form, Input } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";

interface OrderFormProps {
  orderProp: OrderModel;
  readOnly: boolean;
  onSubmit: (order: OrderModel) => void;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  readOnly,
  onSubmit,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<OrderModel>({ defaultValues: orderProp });

  return (
    <Form onSubmit={handleSubmit(onSubmit)} className="mb-3">
      <FormField
        register={register}
        errors={errors}
        name="status"
        label="Status"
        required={true}
        readOnly={readOnly}
      />
      <hr />
      <FormField
        register={register}
        errors={errors}
        name="name"
        label="Name"
        required={true}
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="description"
        label="Description"
        type="textarea"
        required={true}
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="category"
        label="Category"
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="externalReference"
        label="External Reference"
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="notes"
        label="Notes"
        required={false}
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="units"
        label="Units"
        required={true}
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="unitPrice"
        label="Unit Price"
        required={true}
        readOnly={readOnly}
        prepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
      <FormField
        register={register}
        errors={errors}
        name="quantity"
        label="Quantity"
        required={true}
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="installments"
        label="Installments"
        readOnly={readOnly}
      />
      <FormField
        register={register}
        errors={errors}
        name="adjustment"
        label="Adjustment"
        readOnly={readOnly}
        prepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
      <FormField
        register={register}
        errors={errors}
        name="adjustmentReason"
        label="Adjustment Reason"
        readOnly={readOnly}
        type="textarea"
      />
      <FormField
        register={register}
        errors={errors}
        name="adminNotes"
        label="Admin Notes"
        readOnly={readOnly}
        type="textarea"
      />
      <hr />
      <FormField
        register={register}
        errors={errors}
        name="subTotal"
        label="SubTotal"
        required={true}
        readOnly={true}
        prepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
      <FormField
        register={register}
        errors={errors}
        name="total"
        label="Total"
        required={true}
        readOnly={true}
        prepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
      {!readOnly && <Input type="submit" value="Save" />}
    </Form>
  );
};

export default OrderForm;
