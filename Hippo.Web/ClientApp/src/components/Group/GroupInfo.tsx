import { GroupModel } from "../../types";
import { Card, CardSubtitle, CardText, CardTitle } from "reactstrap";

export interface GroupInfoProps {
  group: GroupModel;
}

export const GroupInfo = ({ group }: GroupInfoProps) => {
  return (
    <div className="group-card-admin" key={group.id}>
      <p className="mb-0">
        <b>
          {group.displayName}{" "}
          {group.name !== group.displayName && `(${group.name})`}
        </b>
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
