import React from "react";
import { ArrayPath, useFormContext } from "react-hook-form";
import { OrderMetadataModel, OrderModel } from "../../types";

import FormFieldArray from "../../Shared/Form/FormFieldArray";

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

  return (
    <>
      <FormFieldArray
        register={register}
        control={control}
        errors={errors}
        arrayName={arrayName}
      />
    </>
  );
};

export default OrderFormFieldArray;
