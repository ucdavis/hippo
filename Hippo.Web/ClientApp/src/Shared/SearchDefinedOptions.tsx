import * as React from "react";
import { Typeahead } from "react-bootstrap-typeahead";
import "react-bootstrap-typeahead/css/Typeahead.css";

interface IProps<T extends string> {
  onSelect: (definedOptions: T[]) => void;
  disabled: boolean;
  selected: T[];
  definedOptions: T[];
  placeHolder: string;
  id: string;
}

// user searches and picks from a list of pre-defined options
const SearchDefinedOptions = <T extends string>(props: IProps<T>) => {
  return (
    <div>
      <Typeahead
        id={props.id} // for accessibility
        options={props.disabled ? [] : props.definedOptions}
        disabled={props.disabled}
        multiple={true}
        clearButton={true}
        onChange={(selected) => {
          props.onSelect([...selected] as T[]);
        }}
        selected={props.selected}
        highlightOnlyResult={true}
        placeholder={props.placeHolder}
      />
    </div>
  );
};

export default SearchDefinedOptions;
