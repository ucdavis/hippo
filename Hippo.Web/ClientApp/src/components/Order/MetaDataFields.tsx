import React from "react";
import { useFieldArray, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";

import FormField from "../../Shared/Form/FormField";
import { Row, Col } from "reactstrap";
import HipButton from "../../Shared/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";

type MetaDataFieldsProps = {};

const MetaDataFields: React.FC<MetaDataFieldsProps> = ({}) => {
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
            <Col>
              <FormField
                key={`name-${field.id}`}
                register={register}
                label="Name"
                error={errors.metaData?.[index]?.name}
                name={`metaData.${index}.name`}
                required={true}
                autoComplete="nope"
              />
            </Col>
            <Col>
              <FormField
                key={`value-${field.id}`}
                register={register}
                label="Value"
                error={errors.metaData?.[index]?.value}
                name={`metaData.${index}.value`}
                required={true}
              />
            </Col>
            <Col md={3}>
              <HipButton
                title="Remove Metadata"
                aria-label="Remove Metadata"
                color="danger"
                outline={true}
                size="sm"
                onClick={() => removeMetaData(index)}
              >
                <FontAwesomeIcon icon={faTrash} />
              </HipButton>
            </Col>
          </Row>
        );
      })}
      <HipButton outline={true} color="secondary" onClick={addMetaData}>
        <FontAwesomeIcon icon={faPlus} size="sm" /> Add Metadata
      </HipButton>
    </>
  );
};

export default MetaDataFields;
