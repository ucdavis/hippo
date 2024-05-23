import { FormFieldArrayProps } from "./formTypes";
import { useFieldArray } from "react-hook-form";

const FormFieldArray = <T extends Record<string, any>>({
  control,
  name,
}: FormFieldArrayProps<T>) => {
  const { fields, append, remove } = useFieldArray({
    control,
    name,
  });

  console.log(fields);
  return (
    <>
      {/* {fields.map((field, index) => (
        <FormField
          key={field.id}
          register={register}
          error={error}
          name={`${fields}.${index}` as FieldPath<T>}
          {...options}
        />
      ))} */}
    </>
  );
};

export default FormFieldArray;
