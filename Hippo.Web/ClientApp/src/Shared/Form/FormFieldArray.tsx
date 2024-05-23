import { OrderMetadataModel, OrderModel } from "../../types";
import FormField from "./FormField";
import { FormFieldArrayProps } from "./formTypes";
import { useFieldArray, useFormContext } from "react-hook-form";

const FormFieldArray = <T extends Record<string, any>>({
  control,
  register,
  errors,
  arrayName = "metaData",
}: FormFieldArrayProps<OrderModel>) => {
  const arrayName2 = "metaData";
  // const { control, register } = useFormContext<OrderModel>();
  const { fields, append, remove } = useFieldArray({
    control,
    name: "metaData",
  });

  return (
    <>
      {fields.map((field, index) => {
        console.log("field", field);
        console.log(`[arrayName][${index}]`, [arrayName]);
        return (
          <>
            <FormField
              key={field.id}
              register={register}
              label="Name"
              error={errors.metaData?.[index]?.name}
              name={`${arrayName2}.${index}.name` as const}
              // {...options}
            />
          </>
        );
      })}
    </>
  );
};

export default FormFieldArray;
