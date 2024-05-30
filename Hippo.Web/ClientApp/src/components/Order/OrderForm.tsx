import React from "react";
import { FormProvider, set, useForm } from "react-hook-form";
import { OrderModel, OrderTotalCalculationModel } from "../../types";
import { Form } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import FormSubmitButton from "../../Shared/Form/FormSubmitButton";
import MetaDataFields from "./MetaDataFields";
import OrderFormField from "./OrderFormField";

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
  const methods = useForm<OrderModel>({ defaultValues: orderProp });
  const {
    handleSubmit,
    setError,
    getValues,
    setValue,
    clearErrors,
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

  const updateTotals = (
    field: keyof OrderTotalCalculationModel,
    value: number,
  ) => {
    const data = getValues();
    console.log(data);

    let adjustment = 0.0;
    try {
      adjustment =
        field === "adjustment" ? value : parseFloat(data.adjustment.toString()); // Don't know why, but depending on the order of values changed, this can show up as a string
    } catch (error) {
      adjustment = 0.0;
    }
    const quantity = field === "quantity" ? value : data.quantity;
    const unitPrice =
      field === "unitPrice" ? value : parseFloat(data.unitPrice);

    const subTotal = quantity * unitPrice;
    const total = subTotal + adjustment;
    //const balanceRemaining = total;
    //Possibly show a Installment payment amount

    try {
      setValue("subTotal", subTotal.toFixed(2));
    } catch (error) {
      setValue("subTotal", "0");
    }

    try {
      setValue("total", total.toFixed(2));
    } catch (error) {
      setValue("total", "0");
    }
    clearErrors("total");
    if (total < 0) {
      setError("total", {
        type: "custom",
        message: "Total must be a positive number",
      });
    }
  };

  // TODO: rest of input validation?
  return (
    <FormProvider {...methods}>
      <Form onSubmit={handleSubmit(submitForm)} className="mb-3">
        <fieldset disabled>
          <OrderFormField
            name="status"
            label="Status"
            required={false}
            readOnly={true}
          />
        </fieldset>
        <hr />
        <fieldset disabled={readOnly}>
          <fieldset disabled={!isAdmin}>
            <OrderFormField
              name="productName"
              label="Product Name"
              required={true}
              readOnly={readOnly || !isAdmin}
              maxLength={50}
            />
            <OrderFormField
              name="description"
              label="Description"
              type="textarea"
              required={true}
              readOnly={readOnly || !isAdmin}
              maxLength={250}
            />
            <OrderFormField
              name="category"
              label="Category"
              readOnly={readOnly || !isAdmin}
            />
            <OrderFormField
              name="units"
              label="Units"
              required={true}
              readOnly={readOnly || !isAdmin}
            />

            <OrderFormField
              name="unitPrice"
              label="Unit Price"
              required={true}
              readOnly={readOnly || !isAdmin}
              onChange={(e) =>
                updateTotals("unitPrice", parseFloat(e.target.value))
              }
              inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
            />
            <OrderFormField
              name="installments"
              label="Installments"
              readOnly={readOnly || !isAdmin}
            />
            <OrderFormField
              name="installmentType"
              label="Installment Type"
              readOnly={readOnly || !isAdmin}
              type="select"
            >
              <option value="Monthly">Monthly</option>
              <option value="Yearly">Yearly</option>
            </OrderFormField>
          </fieldset>
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
            onChange={(e) =>
              updateTotals("quantity", parseFloat(e.target.value))
            }
          />
          <fieldset disabled={!isAdmin}>
            <OrderFormField
              name="externalReference"
              label="External Reference"
              readOnly={readOnly || !isAdmin}
              maxLength={150}
            />
            <OrderFormField
              name="adjustment"
              label="Adjustment"
              readOnly={readOnly || !isAdmin}
              onChange={(e) =>
                updateTotals("adjustment", parseFloat(e.target.value))
              }
              inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
            />
            <OrderFormField
              name="adjustmentReason"
              label="Adjustment Reason"
              readOnly={readOnly || !isAdmin}
              type="textarea"
            />
          </fieldset>
          {isAdmin && (
            <OrderFormField
              name="adminNotes"
              label="Admin Notes"
              readOnly={readOnly}
              type="textarea"
            />
          )}
          <MetaDataFields readOnly={readOnly} />
        </fieldset>
        <hr />
        <OrderFormField
          name="subTotal"
          label="SubTotal"
          readOnly={true}
          disabled={true}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
        <OrderFormField
          name="total"
          label="Total"
          readOnly={true}
          disabled={true}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />

        {!readOnly && <FormSubmitButton className="mb-3 mt-3" />}
      </Form>
    </FormProvider>
  );
};

export default OrderForm;
