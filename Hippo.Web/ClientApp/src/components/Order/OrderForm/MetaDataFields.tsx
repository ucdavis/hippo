import { useFieldArray, useFormContext } from "react-hook-form";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";
import HipFormField from "../../../Shared/Form/HipFormField";
import HipButton from "../../../Shared/HipComponents/HipButton";
import { OrderModel } from "../../../types";
import HipDumbTable from "../../../Shared/Table/HipDumbTable";

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
      <br />
      <HipDumbTable>
        <thead>
          <tr>
            <th style={{ width: "45%" }}>Name</th>
            <th style={{ width: "45%" }}>Value</th>
            {!readOnly && <th style={{ width: "10%" }}></th>}
          </tr>
        </thead>
        <tbody>
          {fields.map((field, index) => {
            return (
              <tr key={field.id}>
                <td width={"45%"}>
                  <HipFormField
                    size="lg"
                    key={`name-${field.id}`}
                    register={register}
                    label="Metadata Name"
                    hideLabel={true}
                    error={errors.metaData?.[index]?.name}
                    name={`metaData.${index}.name`}
                    required={true}
                    autoComplete="nope"
                    readOnly={readOnly}
                    maxLength={128}
                  />
                </td>
                <td width={"45%"}>
                  <HipFormField
                    size="lg"
                    key={`value-${field.id}`}
                    register={register}
                    label="Metadata Value"
                    hideLabel={true}
                    error={errors.metaData?.[index]?.value}
                    name={`metaData.${index}.value`}
                    required={true}
                    readOnly={readOnly}
                    maxLength={450}
                  />
                </td>
                {!readOnly && (
                  <td width={"10%"}>
                    <HipButton
                      title="Remove Metadata"
                      aria-label="Remove Metadata"
                      onClick={() => removeMetaData(index)}
                      size="sm"
                      outline={true}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </HipButton>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </HipDumbTable>
      {!readOnly && (
        <HipButton
          className="mb-5"
          color="primary"
          onClick={addMetaData}
          size="sm"
        >
          <FontAwesomeIcon icon={faPlus} /> Add Metadata
        </HipButton>
      )}
    </>
  );
};

export default MetaDataFields;
