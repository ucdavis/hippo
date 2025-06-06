import React, { useState } from "react";
import { useFieldArray, useFormContext, useWatch } from "react-hook-form";
import { OrderModel } from "../../types";

import HipFormField from "../../Shared/Form/HipFormField";
import HipButton from "../../Shared/HipComponents/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash, faSearch } from "@fortawesome/free-solid-svg-icons";
import { authenticatedFetch } from "../../util/api";
import ChartStringValidation from "./ChartStringValidation";

import OrderFormField from "./OrderForm/OrderFormField";
import HipDumbTable from "../../Shared/Table/HipDumbTable";

declare const window: Window &
  typeof globalThis & {
    Finjector: any;
  };

type BillingsFieldsProps = {
  readOnly: boolean;
};

interface ValidationNotification {
  index: number;
  message: string;
}

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
        isValid: null,
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

  const [notification, setNotification] = useState<ValidationNotification>({
    index: null,
    message: "",
  });
  const showNotification = (index: number, message: string) => {
    setNotification({
      index: index,
      message: message,
    });
    // @laholstege TODO: use notification util
    // Optionally, clear the notification after some time
    setTimeout(() => setNotification({ index: null, message: "" }), 5000);
  };

  const lookupChartString = async (index: number) => {
    const existingBilling = getValues("billings")[index];

    const chart = await window.Finjector.findChartSegmentString();

    if (chart.status === "success") {
      //add the chart.data chartString input

      const rtValue = await validateChartString({
        index,
        chartString: chart.data,
      });

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

  const onChartStringBlur = async ({
    index,
    chartString,
  }: {
    index: number;
    chartString: string;
  }) => {
    if (!chartString) {
      return;
    }

    const existingBilling = getValues("billings")[index];

    const rtValue = await validateChartString({ index, chartString });

    update(index, {
      ...existingBilling,
      chartString: rtValue.chartString,
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

  const validateChartString = async ({
    index,
    chartString,
  }: {
    index: number;
    chartString: string;
  }) => {
    let response = await authenticatedFetch(
      `/api/order/validateChartString/${chartString}/Debit`,
      {
        method: "GET",
      },
    );

    if (response.ok) {
      const result = await response.json();
      if (chartString !== result.chartString) {
        showNotification(index, result.warning);
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
    <>
      <h2>Billing Info</h2>
      <br />

      <HipDumbTable>
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
              <tr key={`row_${field.id}`}>
                <td width={"40%"}>
                  <HipFormField
                    size="lg"
                    key={`chartString-${field.id}`}
                    register={register}
                    label="Chart String"
                    hideLabel={true}
                    error={errors.billings?.[index]?.chartString} // RH TODO: have invalid chart string produce form error
                    name={`billings.${index}.chartString`}
                    autoComplete="nope"
                    onBlur={(e) => {
                      onChartStringBlur({ index, chartString: e.target.value });
                    }}
                    readOnly={readOnly}
                    maxLength={128}
                    feedback={
                      index === notification.index ? notification.message : ""
                    }
                    feedbackType="tooltip"
                    valid={!readOnly && field?.chartStringValidation?.isValid}
                  />
                </td>
                <td width={"10%;"}>
                  <HipFormField
                    size="lg"
                    key={`percentage-${field.id}`}
                    register={register}
                    label="Percentage"
                    hideLabel={true}
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
                      outline={true}
                      size="sm"
                      onClick={() => lookupChartString(index)}
                    >
                      <FontAwesomeIcon icon={faSearch} />
                    </HipButton>{" "}
                    <HipButton
                      title="Remove ChartString"
                      aria-label="Remove ChartString"
                      outline={true}
                      size="sm"
                      onClick={() => removeBilling(index)}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </HipButton>
                  </td>
                )}
                {readOnly && (
                  <td width={"15%"}>
                    <a
                      href={`https://finjector.ucdavis.edu/details/${field?.chartString}`}
                      target="_blank"
                      rel="noreferrer"
                    >
                      Details
                    </a>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
        <tfoot>
          <tr>
            <td width={"40%"}>Percent Total</td>
            <td width={"10%"}>
              <OrderFormField
                name="percentTotal"
                label=""
                canEditConditions={false} // can never edit
                type="number"
              />
            </td>
            <td />
            <td width={"15%"} />
          </tr>
        </tfoot>
      </HipDumbTable>

      {!readOnly && (
        <HipButton
          outline={true}
          color="primary"
          size="sm"
          onClick={addBilling}
        >
          <FontAwesomeIcon icon={faPlus} /> Add Billing
        </HipButton>
      )}
    </>
  );
};

export default BillingsFields;
