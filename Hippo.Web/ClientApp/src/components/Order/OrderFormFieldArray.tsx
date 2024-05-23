import React from "react";
import { ArrayPath, useFieldArray, useFormContext } from "react-hook-form";
import { OrderMetadataModel, OrderModel } from "../../types";

import FormField from "../../Shared/Form/FormField";
import { Row, Col, Button } from "reactstrap";
import HipButton from "../../Shared/HipButton";

type OrderFormFieldArrayProps = {};

const OrderFormFieldArray: React.FC<OrderFormFieldArrayProps> = ({}) => {
  const {
    control,
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  const { fields, append, remove } = useFieldArray({
    control,
    name: "metaData",
  });

  const addMetaData = () => {
    append({ id: 0, name: "", value: "" });
  };

  const removeMetaData = (index: number) => {
    remove(index);
  };

  return (
    <>
      <hr />
      <h2>Metadata</h2>
      {fields.map((field, index) => {
        return (
          <Row key={field.id}>
            <Col md={2}>
              <HipButton onClick={() => removeMetaData(index)}>
                Remove
              </HipButton>
            </Col>
            <Col>
              <FormField
                key={`name-${field.id}`}
                register={register}
                label="Name"
                required={true}
                error={errors.metaData?.[index]?.name}
                name={`metaData.${index}.name` as const}
              />
            </Col>
            <Col md={5}>
              <FormField
                key={`value-${field.id}`}
                register={register}
                label="Value"
                error={errors.metaData?.[index]?.value}
                name={`metaData.${index}.value` as const}
              />
            </Col>
          </Row>
        );
      })}
      <HipButton onClick={addMetaData}>Add Metadata</HipButton>
    </>
  );
};

export default OrderFormFieldArray;
