import React from "react";
import { useForm } from "react-hook-form";
import { OrderModel } from "../../types";
import FormField from "../../Shared/Form/FormField";
import { Form } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import FormSubmitButton from "../../Shared/Form/FormSubmitButton";

interface OrderFormProps {
  orderProp: OrderModel;
  readOnly: boolean;
  onSubmit: (order: OrderModel) => Promise<void>;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  readOnly,
  onSubmit,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<OrderModel>({ defaultValues: orderProp });

  // TODO: rest of input validation?
  return (
    <Form onSubmit={handleSubmit(onSubmit)} className="mb-3">
      <FormField
        register={register}
        errors={errors}
        name="status"
        label="Status"
        required={true}
        readOnly={true}
      />
      <hr />
      <FormField
        register={register}
        errors={errors}
        name="name"
        label="Name"
        required={true}
        readOnly={readOnly}
        maxLength={50}
      />
      <FormField
        register={register}
        errors={errors}
        name="productName"
        label="Product Name"
        required={true}
        readOnly={readOnly}
        maxLength={50}
      />
      <FormField
        register={register}
        errors={errors}
        name="description"
        label="Description"
        type="textarea"
        required={true}
        readOnly={readOnly}
        maxLength={250}
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
        maxLength={150}
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
        name="installmentType"
        label="Installment Type"
        readOnly={readOnly}
        disabled={readOnly}
        type="select"
      >
        <option value="Monthly">Monthly</option>
        <option value="Weekly">Weekly</option>
      </FormField>
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
      {!readOnly && (
        <FormSubmitButton
          isSubmitting={isSubmitting}
          isSubmitSuccessful={isSubmitSuccessful}
        />
      )}
    </Form>
  );
};

export default OrderForm;
