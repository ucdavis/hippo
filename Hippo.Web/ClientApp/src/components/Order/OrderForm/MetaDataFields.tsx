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
    <>
      <h2>Metadata</h2>
      <HipFormGroup size="lg">
        {fields.map((field, index) => {
          return (
            <Row key={field.id}>
              <HipFormField
                size="lg"
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
              <HipFormField
                size="lg"
                key={`value-${field.id}`}
                register={register}
                label="Value"
                error={errors.metaData?.[index]?.value}
                name={`metaData.${index}.value`}
                required={true}
                readOnly={readOnly}
                maxLength={450}
              />
              {!readOnly && (
                <HipButton
                  title="Remove Metadata"
                  aria-label="Remove Metadata"
                  color="danger"
                  onClick={() => removeMetaData(index)}
                  size="sm"
                >
                  <FontAwesomeIcon icon={faTrash} />
                </HipButton>
              )}
            </Row>
          );
        })}
      </HipFormGroup>
      {!readOnly && (
        <HipButton color="primary" onClick={addMetaData} size="sm">
          <FontAwesomeIcon icon={faPlus} /> Add Metadata
        </HipButton>
      )}
    </>
  );
};

export default MetaDataFields;
