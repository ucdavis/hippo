import React from "react";
import { ArrayPath, useFieldArray, useFormContext } from "react-hook-form";
import { OrderMetadataModel, OrderModel } from "../../types";

import FormFieldArray from "../../Shared/Form/FormFieldArray";
import FormField from "../../Shared/Form/FormField";

type OrderFormFieldArrayProps = {
  arrayName: any; //ArrayPath<OrderMetadataModel>;
};

const OrderFormFieldArray: React.FC<OrderFormFieldArrayProps> = ({
  arrayName,
}) => {
  const {
    control,
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();
  const { fields, append, remove } = useFieldArray({
    control,
    name: "metaData",
  });

  const keys = Object.keys(fields);
  console.log(keys);

  return (
    <>
      {fields.map((field, index) => {
        return (
          <>
            <FormField
              key={field.id}
              register={register}
              label="Metadata Name"
              error={errors.metaData?.[index]?.name}
              name={`metaData.${index}.name` as const}
              required={true}
              // {...options}
            />
          </>
        );
      })}
    </>
  );
};

export default OrderFormFieldArray;
