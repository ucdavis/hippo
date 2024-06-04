import React from "react";
import { FormProvider, useForm } from "react-hook-form";
import { OrderModel } from "../../types";
import { Form } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import FormSubmitButton from "../../Shared/Form/FormSubmitButton";
import MetaDataFields from "./MetaDataFields";
import OrderFormField from "./OrderFormField";
import OrderFormTotalFields from "./OrderFormTotalFields";

interface OrderFormProps {
  orderProp: OrderModel;
  readOnly: boolean;
  isAdmin: boolean;
  onSubmit: (order: OrderModel) => Promise<void>;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  readOnly,
  isAdmin,
  onSubmit,
}) => {
  const methods = useForm<OrderModel>({
    defaultValues: orderProp,
    mode: "onBlur",
  });
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
          required={false}
          readOnly={true}
          disabled={true}
        />
        <hr />
        <OrderFormField
          name="productName"
          label="Product Name"
          required={true}
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          maxLength={50}
        />
        <OrderFormField
          name="description"
          label="Description"
          type="textarea"
          required={true}
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          maxLength={250}
        />
        <OrderFormField
          name="category"
          label="Category"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
        />
        <OrderFormField
          name="units"
          label="Units"
          required={true}
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
        />

        <OrderFormField
          name="unitPrice"
          label="Unit Price"
          required={true}
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
          valueAsNumber={true}
          deps={"total"}
        />
        <OrderFormField
          name="installments"
          label="Installments"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
        />
        <OrderFormField
          name="installmentType"
          label="Installment Type"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          type="select"
        >
          <option value="Monthly">Monthly</option>
          <option value="Yearly">Yearly</option>
        </OrderFormField>
        <OrderFormField
          name="name"
          label="Name"
          required={true}
          readOnly={readOnly}
          maxLength={50}
        />

        <OrderFormField
          name="notes"
          label="Notes"
          type="textarea"
          required={false}
          readOnly={readOnly}
        />

        <OrderFormField
          name="quantity"
          label="Quantity"
          required={true}
          readOnly={readOnly}
          min={0.000001}
          valueAsNumber={true}
          deps={"total"}
        />

        <OrderFormField
          name="externalReference"
          label="External Reference"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          maxLength={150}
        />
        <OrderFormField
          name="adjustment"
          label="Adjustment"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
          valueAsNumber={true}
          deps={"total"}
        />
        <OrderFormField
          name="adjustmentReason"
          label="Adjustment Reason"
          readOnly={readOnly || !isAdmin}
          disabled={!readOnly && !isAdmin}
          type="textarea"
        />

        {isAdmin && (
          <OrderFormField
            name="adminNotes"
            label="Admin Notes"
            readOnly={readOnly}
            type="textarea"
          />
        )}
        <MetaDataFields readOnly={readOnly} />
        <hr />

        <OrderFormTotalFields />

        {!readOnly && <FormSubmitButton className="mb-3 mt-3" />}
      </Form>
    </FormProvider>
  );
};

export default OrderForm;
