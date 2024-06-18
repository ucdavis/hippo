import React from "react";
import { useFieldArray, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";

import FormField from "../../Shared/Form/FormField";
import { Row, Col } from "reactstrap";
import HipButton from "../../Shared/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";

type BillingsFieldsProps = {
  readOnly: boolean;
};

const BillingsFields: React.FC<BillingsFieldsProps> = ({ readOnly }) => {
  const {
    control,
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  const { fields, append, remove } = useFieldArray({
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

  if (readOnly && fields.length === 0) {
    return null;
  }

  return (
    <>
      <h2>Billing Info</h2>
      {fields.map((field, index) => {
        return (
          <Row key={field.id}>
            <Col>
              <div className="input-group">
                <FormField
                  key={`chartString-${field.id}`}
                  register={register}
                  label="ChartString"
                  error={errors.billings?.[index]?.chartString}
                  name={`billings.${index}.chartString`}
                  autoComplete="nope"
                  readOnly={readOnly}
                />
                <button className="btn btn-primary" type="button">
                  <i className="fas fa-search"></i>
                </button>
              </div>
            </Col>
            <Col>
              <FormField
                key={`percentage-${field.id}`}
                register={register}
                label="Percentage"
                error={errors.billings?.[index]?.percentage}
                name={`billings.${index}.percentage`}
                readOnly={readOnly}
              />
            </Col>
            {!readOnly && (
              <Col md={3}>
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
              </Col>
            )}
          </Row>
        );
      })}
      {!readOnly && (
        <HipButton outline={true} color="secondary" onClick={addBilling}>
          <FontAwesomeIcon icon={faPlus} size="sm" /> Add Billing
        </HipButton>
      )}
    </>
  );
};

export default BillingsFields;
