import React from "react";
import { useFieldArray, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";
import { Input } from "reactstrap";

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
      {fields.map((field, index) => (
        <Input
          key={field.id} // important to include key with field's id
          {...register(`metaData.${index}.name`)}
        />
      ))}
    </>
  );
};

export default OrderFormMetadata;
