; ModuleID = 'repl_module'
source_filename = "repl_module"

@i = global i64 0, align 8
@fmt_int_raw = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_0() {
entry:
  store i64 0, ptr @i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %i_load = load i64, ptr @i, align 8
  %icmp_tmp = icmp slt i64 %i_load, 10
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %i_load1 = load i64, ptr @i, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_int_raw, i64 %i_load1)
  br label %for.step

for.step:                                         ; preds = %for.body
  %x_load = load i64, ptr @i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
