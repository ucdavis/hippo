import React from "react";
import { OrderModel } from "../../types";
import FormField from "../../Shared/Form/FormField";
import { useFormContext } from "react-hook-form";
import { FormFieldProps } from "../../Shared/Form/formTypes";

type OrderFormFieldProps = FormFieldProps<OrderModel>;

const OrderFormField: React.FC<OrderFormFieldProps> = ({ ...props }) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <FormField register={register} error={errors[props.name]} {...props} />
  );
};

export default OrderFormField;
