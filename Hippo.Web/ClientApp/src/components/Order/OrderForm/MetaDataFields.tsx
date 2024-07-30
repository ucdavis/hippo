import { useFieldArray, useFormContext } from "react-hook-form";

import { Row, Col, Label } from "reactstrap";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";
import FormField from "../../../Shared/Form/HipFormField";
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
    <HipFormGroup wrap={false}>
      <Label for="metaData" tag="h2">
        Metadata
      </Label>
      {fields.map((field, index) => {
        return (
          <Row key={field.id} className="no-wrap">
            <Col>
              <FormField
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
              <FormField
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
            )}
          </Row>
        );
      })}
      {!readOnly && (
        <HipButton outline={true} color="secondary" onClick={addMetaData}>
          <FontAwesomeIcon icon={faPlus} size="sm" /> Add Metadata
        </HipButton>
      )}
    </HipFormGroup>
  );
};

export default MetaDataFields;
