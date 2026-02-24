; ModuleID = 'KaleidoscopeModule'
source_filename = "KaleidoscopeModule"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-i128:128-f80:128-n8:16:32:64-S128"

@print_int_fmt = private unnamed_addr constant [4 x i8] c"%d\0A\00", align 1

define i32 @main() {
entry:
  %printcall = call i32 (ptr, ...) @printf(ptr @print_int_fmt, i32 223)
  ret i32 223
}

declare i32 @printf(ptr, ...)
