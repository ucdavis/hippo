import React, { useState } from "react";
import {
  ListGroup,
  ListGroupItem,
  Collapse,
  FormGroup,
  Input,
  Label,
} from "reactstrap";
import { isObject } from "../util/TypeChecks";
import { filterCommonProperties } from "../util/ObjectHelpers";

interface ObjectTreeProps {
  obj: Record<string, any>;
}

const ObjectTree: React.FC<ObjectTreeProps> = ({ obj }) => {
  const [expanded, setExpanded] = useState<{ [key: string]: boolean }>({});
  const [allExpanded, setAllExpanded] = useState(false);

  const toggle = (event: React.MouseEvent) => {
    const id = event.currentTarget.getAttribute("id");
    setExpanded((prevState) => ({ ...prevState, [id]: !prevState[id] }));
  };

  const renderItems = (
    obj: Record<string, any>,
    parentId?: string,
    level = 0,
  ) => {
    const keys = Object.keys(obj);
    return keys.map((key) => {
      const value = obj[key];
      const id = `${key}-${parentId ?? "top"}`;
      const valueIsObject = isObject(value);
      const keyIsNumber = !Number.isNaN(Number.parseInt(key));
      const item = (
        <React.Fragment key={id}>
          <ListGroupItem
            className="border-top-0 border-bottom-0 border-end-0"
            style={{ paddingRight: 0, paddingTop: 2, paddingBottom: 2 }}
          >
            <div
              style={valueIsObject ? { cursor: "pointer" } : {}}
              id={id}
              onClick={toggle}
            >
              {valueIsObject && (
                <i
                  className={
                    expanded[id] || allExpanded
                      ? "fas fa-caret-down"
                      : "fas fa-caret-right"
                  }
                ></i>
              )}{" "}
              {
                // when array element is a collapsed object with a name property, display the name
                keyIsNumber &&
                  valueIsObject &&
                  value.hasOwnProperty("name") &&
                  !expanded[id] &&
                  !allExpanded &&
                  (value["name"] as string)
              }
              {!keyIsNumber && `${key}: `}
              {!valueIsObject && value}
            </div>
            {valueIsObject && (
              <Collapse isOpen={expanded[id] || allExpanded}>
                <ListGroup>{renderItems(value, id, level + 1)}</ListGroup>
              </Collapse>
            )}
          </ListGroupItem>
        </React.Fragment>
      );

      return item;
    });
  };

  return (
    <div>
      <FormGroup switch>
        <Input
          type="switch"
          checked={allExpanded}
          onClick={() => {
            setAllExpanded(!allExpanded);
          }}
        />
        <Label check size="sm">
          Expand All
        </Label>
      </FormGroup>
      <ListGroup flush>{renderItems(filterCommonProperties(obj))}</ListGroup>
    </div>
  );
};

export default ObjectTree;
