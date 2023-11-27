import { GroupModel } from "../../types";
import { Card, CardSubtitle, CardText, CardTitle } from "reactstrap";

export interface GroupInfoProps {
  group: GroupModel;
}

export const GroupInfo = ({ group }: GroupInfoProps) => {
  return (
    <div className="group-info-baseline" key={group.id}>
      <p className="group-info-header">
        {group.displayName}{" "}
        {group.name !== group.displayName && `(${group.name})`}
      </p>
      {group.admins?.length > 0 && (
        <>
          <CardSubtitle>Group Sponsors/Admins:</CardSubtitle>
          <CardText>
            {group.admins?.map((a, i) => (
              <span key={i}>
                {a.name} ({a.email})<br />
              </span>
            ))}
          </CardText>
        </>
      )}
    </div>
  );
};
