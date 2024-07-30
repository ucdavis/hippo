import React, { useState } from "react";
import { Card, CardBody, CardTitle } from "reactstrap";
import { useFieldArray, useFormContext, useWatch } from "react-hook-form";
import { OrderModel } from "../../types";

import FormField from "../../Shared/Form/HipFormField";
import HipButton from "../../Shared/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash, faSearch } from "@fortawesome/free-solid-svg-icons";
import { authenticatedFetch } from "../../util/api";
import ChartStringValidation from "./ChartStringValidation";

import OrderFormField from "./OrderForm/OrderFormField";
import { HipFormGroup } from "../../Shared/Form/HipFormGroup";

declare const window: Window &
  typeof globalThis & {
    Finjector: any;
  };

type BillingsFieldsProps = {
  readOnly: boolean;
};

const BillingsFields: React.FC<BillingsFieldsProps> = ({ readOnly }) => {
  const {
    control,
    register,
    getValues,
    setValue,
    formState: { errors },
  } = useFormContext<OrderModel>();

  const { fields, append, remove, update } = useFieldArray({
    control,
    name: "billings",
  });

  const addBilling = () => {
    const percentTotal = getValues("percentTotal");
    let percentToSet = 100 - percentTotal;
    if (percentToSet < 0) {
      percentToSet = 0;
    }
    if (percentToSet > 100) {
      percentToSet = 100;
    }

    append({
      id: 0,
      chartString: "",
      percentage: percentToSet.toString(),
      chartStringValidation: {
        isValid: true,
        description: "",
        accountManager: "",
        accountManagerEmail: "",
        message: "",
        warning: "",
      },
    });
  };

  const removeBilling = (index: number) => {
    remove(index);
  };

  const [notification, setNotification] = useState<string>("");
  const showNotification = (message: string) => {
    setNotification(message);
    // @laholstege TODO: use notification util
    // Optionally, clear the notification after some time
    setTimeout(() => setNotification(""), 5000);
  };

  const lookupChartString = async (index: number) => {
    const existingBilling = getValues("billings")[index];

    const chart = await window.Finjector.findChartSegmentString();

    if (chart.status === "success") {
      //add the chart.data chartString input

      const rtValue = await validateChartString(chart.data, index);

      update(index, {
        chartString: rtValue.chartString,
        id: existingBilling.id,
        percentage: existingBilling.percentage,
        chartStringValidation: {
          isValid: rtValue.isValid,
          description: rtValue.description,
          accountManager: rtValue.accountManager,
          accountManagerEmail: rtValue.accountManagerEmail,
          message: rtValue.message,
          warning: rtValue.warning,
        },
      });
    } else {
      alert(
        "Unknown error happened. Please try again and let us know if the problem persists.",
      );
    }
  };

  const onChartStringBlur = async (index: number, chartString: string) => {
    if (!chartString) {
      return;
    }

    const existingBilling = getValues("billings")[index];

    const rtValue = await validateChartString(chartString, index);

    update(index, {
      chartString: rtValue.chartString,
      id: existingBilling.id,
      percentage: existingBilling.percentage,
      chartStringValidation: {
        isValid: rtValue.isValid,
        description: rtValue.description,
        accountManager: rtValue.accountManager,
        accountManagerEmail: rtValue.accountManagerEmail,
        message: rtValue.message,
        warning: rtValue.warning,
      },
    });
  };

  const validateChartString = async (chartString: string, index: number) => {
    let response = await authenticatedFetch(
      `/api/order/validateChartString/${chartString}/Debit`,
      {
        method: "GET",
      },
    );

    if (response.ok) {
      const result = await response.json();
      if (chartString !== result.chartString) {
        showNotification(result.warning);
      }
      return result;
    }
    return { isValid: false, message: "Failed to validate chart string" };
  };

  const billings = useWatch({
    control,
    name: "billings",
  });

  const percentTotal = billings.reduce((acc, billing) => {
    const percentage = parseFloat(billing.percentage);
    return isNaN(percentage) ? acc : acc + percentage;
  }, 0);

  setValue("percentTotal", percentTotal);

  if (readOnly && fields.length === 0) {
    return null;
  }

  return (
    <HipFormGroup wrap={false}>
      <h2>Billing Info</h2>
      <h2>Chart Strings</h2>
      {notification && (
        <div>
          <br />
          <Card className="card-danger">
            <CardTitle>
              <h3>The Chart String has been updated!</h3>
            </CardTitle>
            <CardBody>
              <div>{notification}</div>
            </CardBody>
          </Card>
        </div>
      )}
      <table className="table table-bordered table-striped">
        <thead>
          <tr>
            <th>Chart String</th>
            <th>Percent</th>
            <th>Chart String Details</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {fields.map((field, index) => {
            return (
              <tr key={field.id}>
                <td width={"40%"}>
                  <FormField
                    key={`chartString-${field.id}`}
                    register={register}
                    label=""
                    error={errors.billings?.[index]?.chartString}
                    name={`billings.${index}.chartString`}
                    autoComplete="nope"
                    onBlur={(e) => {
                      onChartStringBlur(index, e.target.value);
                    }}
                    readOnly={readOnly}
                    maxLength={128}
                  />
                </td>
                <td width={"10%;"}>
                  <FormField
                    key={`percentage-${field.id}`}
                    register={register}
                    label=""
                    error={errors.billings?.[index]?.percentage}
                    name={`billings.${index}.percentage`}
                    readOnly={readOnly}
                    required={true}
                    max={100}
                    min={0.01}
                  />
                </td>
                <td width={readOnly ? "45%" : "35%"}>
                  <ChartStringValidation
                    chartString={field?.chartString}
                    key={field?.chartString}
                  />
                </td>
                {!readOnly && (
                  <td width={"15%"}>
                    <HipButton
                      title="Lookup ChartString"
                      aria-label="Lookup ChartString"
                      color="primary"
                      outline={true}
                      size="sm"
                      onClick={() => lookupChartString(index)}
                    >
                      <FontAwesomeIcon icon={faSearch} />
                    </HipButton>{" "}
                    <HipButton
                      title="Remove ChartString"
                      aria-label="Remove ChartString"
                      color="danger"
                      outline={true}
                      size="sm"
                      onClick={() => removeBilling(index)}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </HipButton>
                  </td>
                )}
                {readOnly && <td width={"5%"}></td>}
              </tr>
            );
          })}
        </tbody>
        <tfoot>
          <tr>
            <td>Percent Total</td>
            <td colSpan={3}>
              <OrderFormField
                name="percentTotal"
                label=""
                readOnly={true}
                disabled={true}
                type="number"
              />
            </td>
          </tr>
        </tfoot>
      </table>

      {!readOnly && (
        <HipButton outline={true} color="secondary" onClick={addBilling}>
          <FontAwesomeIcon icon={faPlus} size="sm" /> Add Billing
        </HipButton>
      )}
    </HipFormGroup>
  );
};

export default BillingsFields;
