import React, { useState } from "react";
import { FormProvider, useForm, useWatch } from "react-hook-form";
import { OrderModel } from "../../types";
import { Form } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import FormSubmitButton from "../../Shared/Form/FormSubmitButton";
import MetaDataFields from "./MetaDataFields";
import OrderFormField from "./OrderFormField";
import OrderFormTotalFields from "./OrderFormTotalFields";
import { authenticatedFetch } from "../../util/api";
import BillingsFields from "./BillingsFields";

interface OrderFormProps {
  orderProp: OrderModel;
  readOnly: boolean;
  isAdmin: boolean;
  cluster: string;
  onlyChartStrings: boolean;
  onSubmit: (order: OrderModel) => Promise<void>;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  readOnly,
  isAdmin,
  cluster,
  onlyChartStrings,
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

  const [foundPI, setFoundPI] = useState(null);
  const [localInstallmentType, setLocalInstallmentType] = useState(
    methods.getValues("installmentType"),
  );

  //lookup pi value
  const lookupPI = async (pi: string) => {
    if (!pi) {
      setFoundPI("");
      return;
    }
    const response = await authenticatedFetch(
      `/api/${cluster}/order/GetClusterUser/${pi}`,
    );
    if (response.ok) {
      const data = await response.json();
      console.log(data);
      if (data?.name) {
        setFoundPI(`Found User ${data.name} (${data.email})`);
      } else {
        setFoundPI(`Not Found ${pi}`);
      }
    } else {
      setFoundPI(`Not Found ${pi}`);
    }
  };

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

  const installmentType = useWatch({
    control: methods.control,
    name: "installmentType",
  });
  const installments = useWatch({
    control: methods.control,
    name: "installments",
  });

  if (installmentType !== localInstallmentType) {
    setLocalInstallmentType(installmentType);
    if (installmentType === "OneTime" && installments !== 1) {
      methods.setValue("installments", 1);
    }
    if (
      installmentType === "Monthly" &&
      (installments === 1 || installments === 5)
    ) {
      methods.setValue("installments", 60);
    }
    if (
      installmentType === "Yearly" &&
      (installments === 1 || installments === 60)
    ) {
      methods.setValue("installments", 5);
    }
  }
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
        {onlyChartStrings && <BillingsFields readOnly={readOnly} />}
        {!onlyChartStrings && (
          <>
            {isAdmin && !readOnly && (
              <>
                <OrderFormField
                  name="PILookup"
                  label="Order for Sponsor (email or kerb)"
                  readOnly={readOnly}
                  disabled={readOnly}
                  onBlur={(e) => {
                    lookupPI(e.target.value);
                  }}
                />
                {foundPI && <span className="text-muted">{foundPI}</span>}
              </>
            )}

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
              name="installmentType"
              label="Installment Type"
              readOnly={readOnly || !isAdmin}
              disabled={!readOnly && !isAdmin}
              type="select"
            >
              <option value="OneTime">One Time</option>
              <option value="Monthly">Monthly</option>
              <option value="Yearly">Yearly</option>
            </OrderFormField>
            {installmentType !== "OneTime" && (
              <OrderFormField
                name="installments"
                label="Installments"
                readOnly={readOnly || !isAdmin}
                disabled={!readOnly && !isAdmin}
              />
            )}
            <OrderFormField
              name="lifeCycle"
              label="Life Cycle in Months"
              readOnly={readOnly || !isAdmin}
              disabled={!readOnly && !isAdmin}
            />
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
              name="installmentDate"
              label="Installment Date"
              readOnly={readOnly || !isAdmin}
              disabled={!readOnly && !isAdmin}
              type="date"
            />
            <OrderFormField
              name="expirationDate"
              label="Expiration Date"
              readOnly={readOnly || !isAdmin}
              disabled={!readOnly && !isAdmin}
              type="date"
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
            <BillingsFields readOnly={readOnly} />
            <hr />

            <OrderFormTotalFields />
          </>
        )}

        {!readOnly && <FormSubmitButton className="mb-3 mt-3" />}
      </Form>
    </FormProvider>
  );
};

export default OrderForm;
