import React from "react";
import { useFieldArray, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";
import { Input } from "reactstrap";
import FormField from "../../Shared/Form/FormField";

interface OrderFormMetadataProps {
  // Define the props for your component here
}

const OrderFormMetadata: React.FC<OrderFormMetadataProps> = () => {
  // Implement your component logic here

  const { control, register } = useFormContext<OrderModel>();
  const { fields, append, remove } = useFieldArray({
    control,
    name: "metaData",
  });

  return (
    <>
      <FormFieldsArray
        control={control}
        register={register}
        fields={fields}
        append={append}
        remove={remove}
      />
    </>
  );
};

export default OrderFormMetadata;
