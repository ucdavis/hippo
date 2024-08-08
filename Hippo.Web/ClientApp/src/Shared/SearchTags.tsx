import * as React from "react";
import { Typeahead } from "react-bootstrap-typeahead";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { isObject } from "../util/TypeChecks";

interface IProps<T extends string> {
  onSelect: (tags: T[]) => void;
  disabled: boolean;
  options: T[];
  selected: T[];
  placeHolder: string;
  id: string;
}

const SearchTags = <T extends string>(props: IProps<T>) => {
  return (
    <div>
      <Typeahead
        id={props.id} // for accessibility
        allowNew
        options={props.disabled ? [] : props.options}
        disabled={props.disabled}
        multiple={true}
        clearButton={true}
        onChange={(selected) => {
          props.onSelect([
            ...selected.map((s) => (isObject(s) ? s["label"] : s)),
          ] as T[]);
        }}
        selected={props.selected}
        placeholder={props.placeHolder}
      />
    </div>
  );
};

export default SearchTags;
