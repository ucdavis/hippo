import React from "react";
import { FormProvider, useForm } from "react-hook-form";
import { OrderModel } from "../../types";
import { Form } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import FormSubmitButton from "../../Shared/Form/FormSubmitButton";
import OrderFormFieldArray from "./OrderFormFieldArray";
import OrderFormField from "./OrderFormField";

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
  const methods = useForm<OrderModel>({ defaultValues: orderProp });
  const {
    handleSubmit,
    setError,
    formState: { isDirty, isSubmitting },
  } = methods;

  const submitForm = async (data: OrderModel) => {
    if (!isDirty || isSubmitting) {
      // if they've made no changes or we're already submitting, don't submit again
      // the button disables itself in this case, but just to be sure
      return;
    }
    try {
      await onSubmit(data);
    } catch (error) {
      // displayed inside FormSubmitButton
      setError("root", {
        type: "server",
        message: "An error occurred submitting your order, please try again.",
      });
    }
  };

  // TODO: rest of input validation?
  return (
    <FormProvider {...methods}>
      <Form onSubmit={handleSubmit(submitForm)} className="mb-3">
        <OrderFormField
          name="status"
          label="Status"
          required={true}
          readOnly={true}
        />
        <hr />
        <OrderFormField
          name="name"
          label="Name"
          required={true}
          readOnly={readOnly}
          maxLength={50}
        />
        <OrderFormField
          name="description"
          label="Description"
          type="textarea"
          required={true}
          readOnly={readOnly}
          maxLength={250}
        />
        <OrderFormField name="category" label="Category" readOnly={readOnly} />
        <OrderFormField
          name="externalReference"
          label="External Reference"
          readOnly={readOnly}
          maxLength={150}
        />
        <OrderFormField
          name="notes"
          label="Notes"
          required={false}
          readOnly={readOnly}
        />
        <OrderFormField
          name="units"
          label="Units"
          required={true}
          readOnly={readOnly}
        />
        <OrderFormField
          name="unitPrice"
          label="Unit Price"
          required={true}
          readOnly={readOnly}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
        <OrderFormField
          name="quantity"
          label="Quantity"
          required={true}
          readOnly={readOnly}
        />
        <OrderFormField
          name="installments"
          label="Installments"
          readOnly={readOnly}
        />
        <OrderFormField
          name="adjustment"
          label="Adjustment"
          readOnly={readOnly}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
        <OrderFormField
          name="adjustmentReason"
          label="Adjustment Reason"
          readOnly={readOnly}
          type="textarea"
        />
        <OrderFormField
          name="adminNotes"
          label="Admin Notes"
          readOnly={readOnly}
          type="textarea"
        />
        <hr />
        <OrderFormField
          name="subTotal"
          label="SubTotal"
          required={true}
          readOnly={true}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
        <OrderFormField
          name="total"
          label="Total"
          required={true}
          readOnly={true}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
        <OrderFormFieldArray arrayName="metaData" />
        {!readOnly && <FormSubmitButton />}
      </Form>
    </FormProvider>
  );
};

export default OrderForm;
