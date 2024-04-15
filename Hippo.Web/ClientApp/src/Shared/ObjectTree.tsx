import React, { useState } from "react";
import { ListGroup, ListGroupItem, Collapse } from "reactstrap";

interface ObjectTreeProps {
  obj: Record<string, any>;
}

const ObjectTree: React.FC<ObjectTreeProps> = ({ obj }) => {
  const [expanded, setExpanded] = useState<{ [key: string]: boolean }>({});

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
      const isObject = typeof value === "object" && value;
      const item = (
        <React.Fragment key={id}>
          <ListGroupItem className="border-top-0 border-bottom-0 border-end-0">
            <div
              style={isObject ? { cursor: "pointer" } : {}}
              id={id}
              onClick={toggle}
            >
              {isObject && (
                <i
                  className={
                    expanded[id] ? "fas fa-caret-down" : "fas fa-caret-right"
                  }
                ></i>
              )}{" "}
              {key}
              {!isObject && `: ${value}`}
            </div>
            {isObject && (
              <Collapse isOpen={expanded[id]}>
                {renderItems(value, id, level + 1)}
              </Collapse>
            )}
          </ListGroupItem>
        </React.Fragment>
      );

      return item;
    });
  };

  return <ListGroup flush>{renderItems(obj)}</ListGroup>;
};

export default ObjectTree;
