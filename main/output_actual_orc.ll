; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [3 x i8] c"ha\00", align 1
@str.1 = private unnamed_addr constant [3 x i8] c"hi\00", align 1
@str.2 = private unnamed_addr constant [14 x i8] c"That is true!\00", align 1
@fmt_str = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@str.3 = private unnamed_addr constant [10 x i8] c"Not true!\00", align 1
@fmt_str.4 = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

define ptr @main_1() {
entry:
  %strcmp_res = call i32 @strcmp(ptr @str, ptr @str.1)
  %str_eq = icmp eq i32 %strcmp_res, 0
  %andtmp = and i1 %str_eq, true
  br i1 %andtmp, label %then, label %else

then:                                             ; preds = %entry
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_str, ptr @str.2)
  br label %ifcont

else:                                             ; preds = %entry
  %printf_call1 = call i32 (ptr, ...) @printf(ptr @fmt_str.4, ptr @str.3)
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @strcmp(ptr, ptr)

declare noalias ptr @malloc(i64)
