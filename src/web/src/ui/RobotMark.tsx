export function RobotMark({ size = 40, className }: { size?: number; className?: string }) {
  return (
    <img
      className={className ? `robot-mark ${className}` : "robot-mark"}
      src="/assistant-robot.png"
      width={size}
      height={size}
      alt=""
      decoding="async"
    />
  );
}
