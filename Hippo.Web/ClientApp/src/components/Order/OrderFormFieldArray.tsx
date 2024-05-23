import React from "react";
import { ArrayPath, useFieldArray, useFormContext } from "react-hook-form";
import { OrderMetadataModel, OrderModel } from "../../types";

import FormField from "../../Shared/Form/FormField";
import { Row, Col } from "reactstrap";

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

  return (
    <>
      {fields.map((field, index) => {
        return (
          <Row key={field.id}>
            <Col md={6}>
              <FormField
                key={`name-${field.id}`}
                register={register}
                label="Name"
                required={true}
                error={errors.metaData?.[index]?.name}
                name={`metaData.${index}.name` as const}
                // {...options}
              />
            </Col>
            <Col md={6}>
              <FormField
                key={`value-${field.id}`}
                register={register}
                label="Value"
                error={errors.metaData?.[index]?.value}
                name={`metaData.${index}.value` as const}
                // {...options}
              />
            </Col>
          </Row>
        );
      })}
    </>
  );
};

export default OrderFormFieldArray;
