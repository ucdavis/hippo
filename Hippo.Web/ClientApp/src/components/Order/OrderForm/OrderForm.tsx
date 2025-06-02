import React, { useState } from "react";
import { FormProvider, useForm, useWatch } from "react-hook-form";
import { OrderModel } from "../../../types";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign, faSearch } from "@fortawesome/free-solid-svg-icons";
import HipFormSubmitButton from "../../../Shared/Form/HipFormSubmitButton";
import MetaDataFields from "./MetaDataFields";
import OrderFormField from "./OrderFormField";
import OrderFormTotalFields from "./OrderFormTotalFields";
import BillingsFields from "../BillingsFields";
import { ShowFor } from "../../../Shared/ShowFor";
import { HipForm } from "../../../Shared/Form/HipForm";
import { Row } from "reactstrap";
import { OrderStatus } from "../Statuses/status";

interface OrderFormProps {
  orderProp: OrderModel;
  isDetailsPage: boolean;
  isAdmin: boolean;
  cluster: string;
  onlyChartStrings: boolean;
  onSubmit: (order: OrderModel) => Promise<void>;
  lookupPI?: (sponsor: string) => Promise<void>;
  foundPI?: string;
}

const OrderForm: React.FC<OrderFormProps> = ({
  orderProp,
  isDetailsPage,
  isAdmin,
  cluster,
  onlyChartStrings,
  onSubmit,
  lookupPI,
  foundPI,
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

  const [localInstallmentType, setLocalInstallmentType] = useState(
    methods.getValues("installmentType"),
  );
  const [localIsRecurring, setLocalIsRecurring] = useState(
    methods.getValues("isRecurring"),
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

  const isRecurring = useWatch({
    control: methods.control,
    name: "isRecurring",
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

  if (isRecurring !== localIsRecurring) {
    setLocalIsRecurring(isRecurring);
    if (!isRecurring) {
      methods.setValue("installmentType", "Monthly");
      methods.setValue("lifeCycle", 60);
      methods.setValue("installments", 60);
    } else {
      methods.setValue("installmentType", "Monthly");
      methods.setValue("lifeCycle", 0);
      methods.setValue("installments", 0);
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
              {isAdmin &&
                !isDetailsPage &&
                orderProp.id === 0 &&
                lookupPI && ( // only on create
                  <Row>
                    <OrderFormField
                      // the form controls the actual value of the input, but the parent component does the actual lookup
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
                  required={true}
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
                  name="isRecurring"
                  label="Recurring"
                  type="checkbox"
                  canEditConditions={adminCanEditLimitedStatuses}
                />
                <OrderFormField
                  name="installmentType"
                  label="Installment Type"
                  type="select"
                  canEditConditions={adminCanEditLimitedStatuses}
                >
                  {!isRecurring && <option value="OneTime">One Time</option>}
                  <option value="Monthly">Monthly</option>
                  <option value="Yearly">Yearly</option>
                </OrderFormField>
                {installmentType !== "OneTime" && !isRecurring && (
                  <OrderFormField
                    name="installments"
                    label="Installments"
                    canEditConditions={adminCanEditLimitedStatuses}
                  />
                )}
                {!isRecurring && (
                  <OrderFormField
                    name="lifeCycle"
                    label="Life Cycle in Months"
                    canEditConditions={adminCanEditLimitedStatuses}
                  />
                )}
              </Row>
              <br />
              <h2>Order Information</h2>
              <Row>
                {orderProp.id !== 0 && (
                  <>
                    <OrderFormField
                      name="piUser.name"
                      label="Sponsor"
                      canEditConditions={false}
                      inputAppend={
                        <a
                          href={`https://who.ucdavis.edu/detail/${orderProp.piUser.kerberos}`}
                          target="_blank"
                          rel="noreferrer noopener"
                        >
                          <FontAwesomeIcon icon={faSearch} />
                        </a>
                      }
                    />
                    <OrderFormField
                      name="status"
                      label="Status"
                      canEditConditions={false}
                    />
                    {/* <OrderFormField
                      name="wasRateAdjusted"
                      label="Rate Changed"
                      type="checkbox"
                      canEditConditions={false}
                    /> */}
                  </>
                )}
                <OrderFormField
                  name="name"
                  label="Order Name"
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
                  name="externalReference"
                  label="External Reference"
                  maxLength={150}
                  canEditConditions={isAdmin && !isDetailsPage} // admin can edit on all statuses
                  hideIfEmpty={true}
                />
                <OrderFormField
                  name="installmentDate"
                  label="Installment Date"
                  type="date"
                  canEditConditions={isAdmin && !isDetailsPage} // admin can edit on all statuses
                  hideIfEmpty={true}
                />
                {!isRecurring && (
                  <OrderFormField
                    name="expirationDate"
                    label="Expiration Date"
                    type="date"
                    canEditConditions={isAdmin && !isDetailsPage} // admin can edit on all statuses
                    hideIfEmpty={true}
                  />
                )}
                <OrderFormField
                  size="md"
                  name="notes"
                  label="Notes"
                  type="textarea"
                  required={false}
                  canEditConditions={!isDetailsPage && !limitedEditing}
                  hideIfEmpty={true}
                />
                {isAdmin && (
                  <OrderFormField
                    size="md"
                    name="adminNotes"
                    label="Admin Notes (Not visible to Sponsor)"
                    type="textarea"
                    canEditConditions={isAdmin && !isDetailsPage} // admin can edit on all statuses
                    hideIfEmpty={true}
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
              <ShowFor condition={isAdmin && !isDetailsPage && limitedEditing}>
                <>
                  <br />
                  <MetaDataFields readOnly={false} />
                </>
              </ShowFor>
              {(orderProp.status !== OrderStatus.Draft || isAdmin) && (
                <Row>
                  <OrderFormField
                    size="md"
                    name="adjustment"
                    label="Adjustment"
                    canEditConditions={adminCanEditLimitedStatuses}
                    inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
                    valueAsNumber={true}
                    deps={"total"}
                    hideIfEmpty={true}
                  />
                  <OrderFormField
                    size="md"
                    name="adjustmentReason"
                    label={`Adjustment Reason${isAdmin ? " (Visible to Sponor)" : ""}`}
                    canEditConditions={adminCanEditLimitedStatuses}
                    type="textarea"
                    hideIfEmpty={true}
                  />
                </Row>
              )}
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
