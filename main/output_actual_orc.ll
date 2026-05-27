; ModuleID = 'repl_module'
source_filename = "repl_module"

%array = type { i64, i64, ptr }
%dataframe = type { ptr, ptr, ptr }
%struct_name_age_hasJob_savings = type { ptr, i64, i8, double }

@x = external global ptr
@__map_src = external global ptr, align 8
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8

define ptr @main_3() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 800)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 100, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %len_ptr1 = getelementptr inbounds nuw %array, ptr %rows_ptr, i32 0, i32 0
  %len = load i64, ptr %len_ptr1, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %len
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_src_load2 = load ptr, ptr @__map_src, align 8
  %__map_i_load3 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load2, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__map_i_load3
  %record = load ptr, ptr %elem_ptr, align 8
  %ptr_age = getelementptr %struct_name_age_hasJob_savings, ptr %record, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 8
  %addtmp = add i64 %val_age, 100
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 1
  %data_ptr_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 2
  %len7 = load i64, ptr %len_ptr4, align 8
  %cap = load i64, ptr %cap_ptr5, align 8
  %data8 = load ptr, ptr %data_ptr_ptr6, align 8
  %is_full = icmp uge i64 %len7, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load9 = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load9, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load10 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__map_result_load10, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %for.body
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data8, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr5, align 8
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %data_phi = phi ptr [ %data8, %for.body ], [ %realloc, %grow ]
  %slot = getelementptr i64, ptr %data_phi, i64 %len7
  store i64 %addtmp, ptr %slot, align 8
  %new_len = add i64 %len7, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
