import { useFieldArray, useFormContext } from "react-hook-form";

import { Row, Col, Label } from "reactstrap";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";
import HipFormField from "../../../Shared/Form/HipFormField";
import HipButton from "../../../Shared/HipButton";
import { OrderModel } from "../../../types";
import { HipFormGroup } from "../../../Shared/Form/HipFormGroup";

type MetaDataFieldsProps = {
  readOnly: boolean;
};

const MetaDataFields: React.FC<MetaDataFieldsProps> = ({ readOnly }) => {
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

  if (readOnly && fields.length === 0) {
    return null;
  }

  return (
    <HipFormGroup size="lg">
      <Label for="metaData" tag="h3">
        Metadata
      </Label>
      {fields.map((field, index) => {
        return (
          <Row key={field.id}>
            <Col>
              <HipFormField
                key={`name-${field.id}`}
                register={register}
                label="Name"
                error={errors.metaData?.[index]?.name}
                name={`metaData.${index}.name`}
                required={true}
                autoComplete="nope"
                readOnly={readOnly}
                maxLength={128}
              />
            </Col>
            <Col>
              <HipFormField
                key={`value-${field.id}`}
                register={register}
                label="Value"
                error={errors.metaData?.[index]?.value}
                name={`metaData.${index}.value`}
                required={true}
                readOnly={readOnly}
                maxLength={450}
              />
            </Col>
            {!readOnly && (
              <Col md={1}>
                <HipButton
                  title="Remove Metadata"
                  aria-label="Remove Metadata"
                  color="danger"
                  onClick={() => removeMetaData(index)}
                  size="sm"
                >
                  <FontAwesomeIcon icon={faTrash} />
                </HipButton>
              </Col>
            )}
          </Row>
        );
      })}
      {!readOnly && (
        <HipButton color="primary" onClick={addMetaData} size="sm">
          <FontAwesomeIcon icon={faPlus} /> Add Metadata
        </HipButton>
      )}
    </HipFormGroup>
  );
};

export default MetaDataFields;
