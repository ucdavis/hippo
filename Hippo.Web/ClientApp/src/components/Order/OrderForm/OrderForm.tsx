import React, { useState } from "react";
import { FormProvider, useForm, useWatch } from "react-hook-form";
import { OrderModel } from "../../../types";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import HipFormSubmitButton from "../../../Shared/Form/HipFormSubmitButton";
import MetaDataFields from "./MetaDataFields";
import OrderFormField from "./OrderFormField";
import OrderFormTotalFields from "./OrderFormTotalFields";
import { authenticatedFetch } from "../../../util/api";
import BillingsFields from "../BillingsFields";
import { ShowFor } from "../../../Shared/ShowFor";
import { HipForm } from "../../../Shared/Form/HipForm";
import { Row } from "reactstrap";

interface OrderFormProps {
  orderProp: OrderModel;
  isDetailsPage: boolean;
  isAdmin: boolean;
  cluster: string;
  onlyChartStrings: boolean;
  onSubmit: (order: OrderModel) => Promise<void>;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  isDetailsPage,
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
    setValue,
    formState: { isDirty, isSubmitting },
  } = methods;

  const [foundPI, setFoundPI] = useState(null);
  const [localInstallmentType, setLocalInstallmentType] = useState(
    methods.getValues("installmentType"),
  );
  /**
   * Editing is limited when order status is Active, Rejected, Cancelled, or Completed
   */
  const [limitedEditing, setLimitedEditing] = useState(false);

  React.useEffect(() => {
    const newStatus = orderProp.status;
    setValue("status", newStatus);

    if (
      newStatus === "Active" ||
      newStatus === "Rejected" ||
      newStatus === "Cancelled" ||
      newStatus === "Completed"
    ) {
      setLimitedEditing(true);
    } else {
      setLimitedEditing(false);
    }

    const newInstallmentDate = orderProp.installmentDate;
    const newExpirationDate = orderProp.expirationDate;
    setValue("installmentDate", newInstallmentDate);
    setValue("expirationDate", newExpirationDate);
  }, [
    setValue,
    orderProp.expirationDate,
    orderProp.installmentDate,
    orderProp.status,
  ]);

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
  const adminCanEditLimitedStatuses =
    isAdmin && !isDetailsPage && !limitedEditing;

  // TODO: rest of input validation?
  return (
    <>
      <FormProvider {...methods}>
        <HipForm onSubmit={handleSubmit(submitForm)}>
          {onlyChartStrings && <BillingsFields readOnly={isDetailsPage} />}
          {!onlyChartStrings && (
            <>
              {isAdmin && !isDetailsPage && orderProp.id === 0 && (
                <Row>
                  <OrderFormField
                    name="PILookup"
                    size="lg"
                    label="Order for Sponsor (email or kerb)"
                    onBlur={(e) => {
                      lookupPI(e.target.value);
                    }}
                    feedback={foundPI}
                    canEditConditions={
                      isAdmin && !isDetailsPage && orderProp.id === 0
                    }
                  />
                </Row>
              )}
              <h2>Product Information</h2>
              <Row>
                <OrderFormField
                  name="productName"
                  label="Product Name"
                  required={true}
                  maxLength={50}
                  canEditConditions={adminCanEditLimitedStatuses}
                />
                <OrderFormField
                  name="description"
                  label="Description"
                  type="textarea"
                  required={true}
                  canEditConditions={adminCanEditLimitedStatuses}
                  maxLength={250}
                />
                <OrderFormField
                  name="category"
                  label="Category"
                  canEditConditions={adminCanEditLimitedStatuses}
                  maxLength={50}
                />
                <OrderFormField
                  name="units"
                  label="Units"
                  required={true}
                  canEditConditions={adminCanEditLimitedStatuses}
                  maxLength={50}
                />
              </Row>
              <Row>
                <OrderFormField
                  name="unitPrice"
                  label="Unit Price"
                  required={true}
                  inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
                  valueAsNumber={true}
                  deps={"total"}
                  canEditConditions={adminCanEditLimitedStatuses}
                />
                <OrderFormField
                  name="installmentType"
                  label="Installment Type"
                  type="select"
                  canEditConditions={adminCanEditLimitedStatuses}
                >
                  <option value="OneTime">One Time</option>
                  <option value="Monthly">Monthly</option>
                  <option value="Yearly">Yearly</option>
                </OrderFormField>
                {installmentType !== "OneTime" && (
                  <OrderFormField
                    name="installments"
                    label="Installments"
                    canEditConditions={adminCanEditLimitedStatuses}
                  />
                )}
                <OrderFormField
                  name="lifeCycle"
                  label="Life Cycle in Months"
                  canEditConditions={adminCanEditLimitedStatuses}
                />
              </Row>
              <br />
              <h2>Order Information</h2>
              <Row>
                <OrderFormField
                  name="name"
                  label="Name"
                  required={true}
                  maxLength={50}
                  canEditConditions={!isDetailsPage && !limitedEditing}
                />
                <OrderFormField
                  name="quantity"
                  label="Quantity"
                  required={true}
                  min={0.01}
                  valueAsNumber={true}
                  deps={"total"}
                  canEditConditions={!isDetailsPage && !limitedEditing}
                />
                <OrderFormField
                  name="installmentDate"
                  label="Installment Date"
                  type="date"
                  canEditConditions={isAdmin && !isDetailsPage} // can edit on all statuses
                />
                <OrderFormField
                  name="expirationDate"
                  label="Expiration Date"
                  type="date"
                  canEditConditions={isAdmin && !isDetailsPage} // can edit on all statuses
                />
                <OrderFormField
                  name="externalReference"
                  label="External Reference"
                  maxLength={150}
                  canEditConditions={adminCanEditLimitedStatuses}
                />
              </Row>
              <Row>
                <OrderFormField
                  size="md"
                  name="notes"
                  label="Notes"
                  type="textarea"
                  required={false}
                  canEditConditions={!isDetailsPage && !limitedEditing}
                />
                {isAdmin && (
                  <OrderFormField // TODO: line up in grid
                    size="md"
                    name="adminNotes"
                    label="Admin Notes"
                    type="textarea"
                    canEditConditions={isAdmin && !isDetailsPage} // can edit on all statuses
                  />
                )}
              </Row>
              <ShowFor condition={isDetailsPage || !limitedEditing}>
                <>
                  <br />
                  <MetaDataFields readOnly={isDetailsPage} />
                  <br />
                  <BillingsFields readOnly={isDetailsPage} />
                  <br />
                </>
              </ShowFor>
              <Row>
                <OrderFormField
                  size="md"
                  name="adjustment"
                  label="Adjustment"
                  canEditConditions={adminCanEditLimitedStatuses}
                  inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
                  valueAsNumber={true}
                  deps={"total"}
                />
                <OrderFormField
                  size="md"
                  name="adjustmentReason"
                  label="Adjustment Reason"
                  canEditConditions={adminCanEditLimitedStatuses}
                  type="textarea"
                />
              </Row>
              <OrderFormTotalFields />
            </>
          )}

          {!isDetailsPage && <HipFormSubmitButton className="mb-3 mt-3" />}
        </HipForm>
      </FormProvider>
    </>
  );
};

export default OrderForm;
