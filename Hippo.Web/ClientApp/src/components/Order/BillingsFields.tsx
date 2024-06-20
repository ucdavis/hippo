import React from "react";
import { useFieldArray, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";

import FormField from "../../Shared/Form/FormField";
import HipButton from "../../Shared/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash, faSearch } from "@fortawesome/free-solid-svg-icons";

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
    formState: { errors },
  } = useFormContext<OrderModel>();

  const { fields, append, remove, update } = useFieldArray({
    control,
    name: "billings",
  });

  const addBilling = () => {
    append({
      id: 0,
      chartString: "",
      percentage: "",
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

  const lookupChartString = async (index: number) => {
    const chart = await window.Finjector.findChartSegmentString();

    if (chart.status === "success") {
      //add the chart.data chartString input
      fields[index].chartString = chart.data;
      update(index, fields[index]);

      // financialDetail.chartString = chart.data;

      // setFinancialDetail((prevFinancialDetail) => ({
      //   ...prevFinancialDetail,
      //   chartString: chart.data,
      // }));

      // await validateChartString(chart.data);
    } else {
      alert("Failed!");
    }
  };

  if (readOnly && fields.length === 0) {
    return null;
  }

  return (
    <>
      <h2>Billing Info</h2>
      <h2>Chart Strings</h2>
      <table className="table table-bordered table-striped">
        <thead>
          <tr>
            <th>Chart String</th>
            <th>Percent</th>
            <th>Chart String Validation</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {fields.map((field, index) => {
            return (
              <tr key={field.id}>
                <td>
                  <FormField
                    key={`chartString-${field.id}`}
                    register={register}
                    label=""
                    error={errors.billings?.[index]?.chartString}
                    name={`billings.${index}.chartString`}
                    autoComplete="nope"
                    readOnly={readOnly}
                  />
                </td>
                <td width={"40px;"}>
                  <FormField
                    key={`percentage-${field.id}`}
                    register={register}
                    label=""
                    error={errors.billings?.[index]?.percentage}
                    name={`billings.${index}.percentage`}
                    readOnly={readOnly}
                  />
                </td>
                <td></td>
                {!readOnly && (
                  <td width={"160px;"}>
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
                {readOnly && <td width={"40px;"}></td>}
              </tr>
            );
          })}
        </tbody>
      </table>

      {!readOnly && (
        <HipButton outline={true} color="secondary" onClick={addBilling}>
          <FontAwesomeIcon icon={faPlus} size="sm" /> Add Billing
        </HipButton>
      )}
    </>
  );
};

export default BillingsFields;
